using System;
using System.Collections.Generic;
using CyberUnderground.Simulation;
using Xunit;

namespace CyberUnderground.Tests
{
    public class PrototypeTests
    {
        static readonly string[] Banned =
        {
            "nmap", "sqlmap", "metasploit", "msfvenom", "hydra", "hashcat",
            "meterpreter", "shellcode", "mimikatz", "cve-", "exploit-db",
            "/etc/passwd", "powershell"
        };

        [Fact]
        public void NewGame_StartsBroke()
        {
            var game = GameSession.NewGame();
            Assert.Equal(17, game.Player.MoneyEuros);
            Assert.Equal(0, game.Reputation.Total);
            Assert.Equal(0, game.Heat.Value);
            Assert.Equal(4, game.Hardware.RamGb);
            Assert.Equal(3, game.Hardware.MaxWindows);
            Assert.Equal(1, game.Skills.Level(SkillBranch.Osint));
        }

        [Fact]
        public void FirstMission_PaysEnoughForRam()
        {
            var game = GameSession.NewGame();
            Assert.False(game.Execute("report campus_survey").Ok);
            game.Visit("www.lumen.edu/it");
            Assert.Contains("TARGET DISCOVERED", game.Execute("scan lumen").Text);
            var filed = game.Execute("report campus_survey");
            Assert.True(filed.Ok);
            Assert.Equal(57, game.Player.MoneyEuros);
            Assert.True(game.Skills.Level(SkillBranch.Osint) >= 2);
            Assert.False(game.Execute("report campus_survey").Ok);

            var bought = game.Execute("upgrade ram_8");
            Assert.True(bought.Ok);
            Assert.Equal(12, game.Player.MoneyEuros);
            Assert.Equal(8, game.Hardware.RamGb);
            Assert.Equal(5, game.Hardware.MaxWindows);
            Assert.False(game.Execute("upgrade ram_8").Ok);
        }

        [Fact]
        public void Footprint_RejectsUnsupportedLinks()
        {
            var game = PreparedFootprint();
            Assert.False(game.Execute("connect mara authlite").Ok);
            Assert.True(game.Execute("connect mara mara_mail").Ok);
            Assert.True(game.Execute("connect novamart dev_site").Ok);
            Assert.True(game.Execute("connect dev_site authlite").Ok);
            Assert.False(game.Execute("connect mara mara_mail").Ok);
            var filed = game.Execute("report novamart_footprint");
            Assert.True(filed.Ok);
            Assert.Equal(137, game.Player.MoneyEuros);
            Assert.True(game.Skills.Level(SkillBranch.Osint) >= 3);
        }

        [Fact]
        public void Connect_StaysLockedUntilCampusSurvey()
        {
            var game = GameSession.NewGame();
            game.Execute("scan novamart");
            Assert.False(game.Execute("connect mara mara_mail").Ok);
        }

        [Fact]
        public void SellRaisesHeat_ReportDoesNot()
        {
            var legal = PreparedFootprint();
            FinishMap(legal);
            legal.Execute("choose novamart_decision report");
            Assert.Equal(0, legal.Heat.Value);
            Assert.True(legal.Reputation.Get(RepKind.Corporate) > 0);

            var shady = PreparedFootprint();
            FinishMap(shady);
            int before = shady.Player.MoneyEuros;
            shady.Execute("choose novamart_decision sell");
            Assert.True(shady.Heat.Value >= 20);
            Assert.True(shady.Reputation.Get(RepKind.Underground) > 0);
            Assert.True(shady.Player.MoneyEuros > before);

            shady.Execute("cool");
            Assert.True(shady.Heat.Value < 24);
            Assert.Equal(before + 180 - 10, shady.Player.MoneyEuros);
        }

        [Fact]
        public void Quiz_AcceptsOnlyTheLookalike()
        {
            var game = GameSession.NewGame();
            FinishCampus(game);
            Assert.False(game.SubmitQuiz("helio", "library").Ok);
            Assert.False(game.Missions.IsComplete("helio_awareness"));
            Assert.True(game.SubmitQuiz("helio", "lookalike").Ok);
            Assert.True(game.Missions.IsComplete("helio_awareness"));
            Assert.True(game.Skills.Level(SkillBranch.Social) >= 1);
        }

        [Fact]
        public void FullAnalysis_NeedsCpu_AndChangesTheWarning()
        {
            var partial = PreparedFootprint();
            FinishMap(partial);
            var thin = partial.Execute("analyze nova");
            Assert.True(thin.Ok);
            Assert.Contains("partial", thin.Text);
            Assert.DoesNotContain("9 days", thin.Text);

            var full = PreparedFootprint();
            FinishMap(full);
            full.Execute("choose novamart_decision report");
            full.SubmitQuiz("helio", "lookalike");
            Assert.True(full.Execute("upgrade cpu_refurb").Ok);
            var deep = full.Execute("analyze nova");
            Assert.Contains("9 days", deep.Text);
            full.Visit("nightwire.social");
            int intel = full.Reputation.Get(RepKind.Intelligence);
            full.Execute("choose nova_rumor warn");
            Assert.True(full.Reputation.Get(RepKind.Intelligence) - intel >= 12);
        }

        [Fact]
        public void Amplify_AfterFullReading_AddsMoreHeat()
        {
            var game = PreparedFootprint();
            FinishMap(game);
            game.Execute("choose novamart_decision hold");
            game.SubmitQuiz("helio", "lookalike");
            game.Execute("upgrade cpu_refurb");
            game.Execute("analyze nova");
            game.Visit("nightwire.social");
            game.Execute("choose nova_rumor amplify");
            Assert.True(game.Heat.Value >= 28);
            Assert.True(game.Missions.IsComplete("nova_rumor"));
        }

        [Fact]
        public void LegalRoute_ClearsFiveMissions_WithoutHeat()
        {
            var game = GameSession.NewGame();
            FinishCampus(game);
            game.Visit("www.novamart.com");
            game.Visit("www.novamart.com/careers");
            game.Visit("www.novamart.com/dev-notes");
            Assert.True(game.Execute("connect mara mara_mail").Ok);
            Assert.True(game.Execute("connect novamart dev_site").Ok);
            Assert.True(game.Execute("connect dev_site authlite").Ok);
            Assert.True(game.Execute("report novamart_footprint").Ok);
            Assert.True(game.Execute("choose novamart_decision report").Ok);
            Assert.True(game.SubmitQuiz("helio", "lookalike").Ok);
            game.Visit("nightwire.social");
            Assert.True(game.Execute("analyze nova").Ok);
            Assert.True(game.Execute("choose nova_rumor warn").Ok);
            Assert.Equal(0, game.Heat.Value);
            Assert.Equal(417, game.Player.MoneyEuros);
            Assert.True(game.Player.MoneyEuros < PlayerManager.SemesterGoal);
        }

        [Fact]
        public void SaveLoad_RoundTrip()
        {
            var game = PreparedFootprint();
            FinishMap(game);
            game.Execute("choose novamart_decision sell");
            string json = game.ExportSave();

            var other = GameSession.NewGame();
            string error;
            Assert.True(other.TryImportSave(json, out error));
            Assert.Equal(game.Player.MoneyEuros, other.Player.MoneyEuros);
            Assert.Equal(game.Heat.Value, other.Heat.Value);
            Assert.True(other.Missions.IsComplete("novamart_decision"));
            Assert.True(other.Osint.HasLink("mara", "mara_mail"));
            Assert.True(other.News.Items.Count > 2);

            Assert.False(other.TryImportSave("nope", out error));
            Assert.Equal(game.Player.MoneyEuros, other.Player.MoneyEuros);
        }

        [Fact]
        public void AuthoredText_StaysFictional()
        {
            var blobs = new List<string>();
            foreach (var line in WorldCatalog.Create().AuditStrings())
                blobs.Add(line);

            var game = GameSession.NewGame();
            blobs.Add(game.Execute("help").Text);
            blobs.Add(game.Execute("scan lumen").Text);
            blobs.Add(game.Execute("scan novamart").Text);
            blobs.Add(game.Execute("scan heliobank").Text);
            FinishCampus(game);
            blobs.Add(game.Execute("help").Text);

            for (int i = 0; i < blobs.Count; i++)
            {
                string lower = blobs[i].ToLowerInvariant();
                for (int b = 0; b < Banned.Length; b++)
                    Assert.DoesNotContain(Banned[b], lower);
            }
        }

        static void FinishCampus(GameSession game)
        {
            game.Visit("www.lumen.edu/it");
            Assert.True(game.Execute("scan lumen").Ok);
            Assert.True(game.Execute("report campus_survey").Ok);
        }

        static GameSession PreparedFootprint()
        {
            var game = GameSession.NewGame();
            FinishCampus(game);
            Assert.True(game.Execute("scan novamart").Ok);
            Assert.True(game.Execute("lookup mara").Ok);
            Assert.True(game.Execute("lookup email").Ok);
            Assert.True(game.Execute("lookup staging").Ok);
            Assert.True(game.Execute("lookup authlite").Ok);
            return game;
        }

        static void FinishMap(GameSession game)
        {
            Assert.True(game.Execute("connect mara mara_mail").Ok);
            Assert.True(game.Execute("connect novamart dev_site").Ok);
            Assert.True(game.Execute("connect dev_site authlite").Ok);
            Assert.True(game.Execute("report novamart_footprint").Ok);
        }
    }
}

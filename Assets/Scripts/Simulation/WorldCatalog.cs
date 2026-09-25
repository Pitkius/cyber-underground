namespace CyberUnderground.Simulation
{
    public static class WorldCatalog
    {
        public static WorldData Create()
        {
            return new WorldData
            {
                Companies = Companies(),
                Pages = Pages(),
                Nodes = Nodes(),
                Edges = Edges(),
                Missions = Missions(),
                Upgrades = Upgrades(),
                StartingNews = News(),
                Quizzes = Quizzes()
            };
        }

        static CompanyDef[] Companies()
        {
            return new[]
            {
                new CompanyDef
                {
                    Id = "lumen",
                    Name = "Lumen University",
                    Blurb = "Your campus. The IT desk pays students for public survey work.",
                    Aliases = new[] { "lumen", "campus", "university" },
                    NodeId = null,
                    ScanReport =
                        "TARGET DISCOVERED\n\n" +
                        "Name: Lumen University\n" +
                        "Kind: Campus\n" +
                        "Infrastructure: 2 fictional services (portal, mail)\n" +
                        "Security: Ordinary\n\n" +
                        "Public signals:\n" +
                        "• Student IT posted a paid survey of this portal\n" +
                        "• The portal software is called LectureHub\n" +
                        "• Nothing here asks you to sign in as someone else\n\n" +
                        "This is a public reading, not an intrusion."
                },
                new CompanyDef
                {
                    Id = "novamart",
                    Name = "NovaMart",
                    Blurb = "Grayhaven delivery retailer. Students shop here and sometimes work the night shift.",
                    Aliases = new[] { "novamart", "nova", "shop" },
                    NodeId = "novamart",
                    ScanReport =
                        "TARGET DISCOVERED\n\n" +
                        "Name: NovaMart\n" +
                        "Kind: Online retailer\n" +
                        "Infrastructure: 3 fictional services (shop, mail, staging)\n" +
                        "Security: Low\n\n" +
                        "Possible weaknesses, taken from public posts:\n" +
                        "• outdated authentication mentioned by staff\n" +
                        "• a development site linked from the careers page\n" +
                        "• employee security notes are vague\n\n" +
                        "Next: read the public pages, or look the names up in your notes.\n" +
                        "Do not try to log in. The work is to map what they already published."
                },
                new CompanyDef
                {
                    Id = "heliobank",
                    Name = "Helio Mutual",
                    Blurb = "Student credit union used by Lumen. Northline Security runs their training inbox.",
                    Aliases = new[] { "heliobank", "helio", "bank" },
                    NodeId = null,
                    ScanReport =
                        "TARGET DISCOVERED\n\n" +
                        "Name: Helio Mutual\n" +
                        "Kind: Student credit union\n" +
                        "Infrastructure: 2 fictional services (app, mail)\n" +
                        "Security: Medium\n\n" +
                        "Public signals:\n" +
                        "• They say staff never ask for a PIN in a message\n" +
                        "• Northline Security grades a training inbox for them\n" +
                        "• No public staging note\n\n" +
                        "This is a public reading, not an intrusion."
                }
            };
        }

        static WebPage[] Pages()
        {
            return new[]
            {
                new WebPage
                {
                    Url = "nexus.search",
                    Title = "Nexus start",
                    Body =
                        "Local start page. These names exist only inside the simulation.\n\n" +
                        "Your balance is not on the desktop. Open the bank, then type a store if you need parts.\n\n" +
                        "Bank: helio.bank/account\n" +
                        "Parts: parts.grayhaven.shop\n" +
                        "Campus work: www.lumen.edu/it",
                    Links = new[]
                    {
                        new WebLink("Helio account", "helio.bank/account"),
                        new WebLink("Grayhaven Parts", "parts.grayhaven.shop"),
                        new WebLink("Lumen University", "www.lumen.edu/it"),
                        new WebLink("NovaMart", "www.novamart.com"),
                        new WebLink("Northline Security", "northline.security"),
                        new WebLink("Nightwire", "nightwire.social"),
                        new WebLink("Daily Grid", "dailygrid.news")
                    }
                },
                new WebPage
                {
                    Url = "www.lumen.edu/it",
                    Title = "Lumen University — Student IT",
                    Body =
                        "The student IT desk is paying for a public map of this portal.\n\n" +
                        "Nothing here is secret. From the terminal, run:\n" +
                        "scan lumen\n" +
                        "report campus_survey\n\n" +
                        "Pay: €40.\n\n" +
                        "The portal runs on LectureHub. It is slow. It is not a target.",
                    Links = new[]
                    {
                        new WebLink("Start", "nexus.search"),
                        new WebLink("Daily Grid", "dailygrid.news")
                    }
                },
                new WebPage
                {
                    Url = "www.novamart.com",
                    Title = "NovaMart",
                    Body =
                        "NovaMart delivers groceries across Grayhaven.\n\n" +
                        "The careers page lists night-shift students. A staging note is still linked from the public site. " +
                        "Staff have not taken it down. Your job, if you take it, is to connect the public facts — not to sign in.",
                    Links = new[]
                    {
                        new WebLink("Careers", "www.novamart.com/careers"),
                        new WebLink("Staging note", "www.novamart.com/dev-notes"),
                        new WebLink("Start", "nexus.search")
                    },
                    Reveals = new[] { "novamart" }
                },
                new WebPage
                {
                    Url = "www.novamart.com/careers",
                    Title = "NovaMart careers",
                    Body =
                        "Night shift, campus edition.\n\n" +
                        "Mara Ellison, IT intern.\n" +
                        "Mailbox printed on the page: m.ellison@novamart.com\n\n" +
                        "The listing also names a public staging host: dev.novamart.com.\n" +
                        "It says to ask Mara how ShelfStack is set up there.",
                    Links = new[]
                    {
                        new WebLink("Staging note", "www.novamart.com/dev-notes"),
                        new WebLink("NovaMart home", "www.novamart.com")
                    },
                    Reveals = new[] { "mara", "mara_mail", "dev_site", "novamart" }
                },
                new WebPage
                {
                    Url = "www.novamart.com/dev-notes",
                    Title = "NovaMart staging note",
                    Body =
                        "Public staging note. It should not still be linked. It is.\n\n" +
                        "dev.novamart.com runs ShelfStack 2.1 with AuthLite 1.0.\n" +
                        "AuthLite 1.0 is the old login module. Staff already described it here.\n\n" +
                        "Do not try to log in. Map the public facts, then decide what to do with the map.",
                    Links = new[]
                    {
                        new WebLink("Careers", "www.novamart.com/careers"),
                        new WebLink("NovaMart home", "www.novamart.com")
                    },
                    Reveals = new[] { "shelstack", "authlite", "dev_site" }
                },
                new WebPage
                {
                    Url = "helio.bank",
                    Title = "Helio Mutual",
                    Body =
                        "Helio Mutual is the student credit union.\n\n" +
                        "Posted policy: we never ask for a vault PIN by message. Use the Helio app you already installed.\n\n" +
                        "Northline Security is hiring a student to grade a training inbox.",
                    Links = new[]
                    {
                        new WebLink("Your account", "helio.bank/account"),
                        new WebLink("Training inbox", "northline.security/inbox"),
                        new WebLink("Northline", "northline.security")
                    }
                },
                new WebPage
                {
                    Url = "northline.security",
                    Title = "Northline Security",
                    Body =
                        "Small Grayhaven firm. They do defensive reviews for Helio Mutual.\n\n" +
                        "Open contract: grade one training inbox. Pick the message that is trying to rush a student. €120.",
                    Links = new[]
                    {
                        new WebLink("Training inbox", "northline.security/inbox"),
                        new WebLink("Helio Mutual", "helio.bank")
                    }
                },
                new WebPage
                {
                    Url = "northline.security/inbox",
                    Title = "Training inbox",
                    Body = "Three fictional messages. Mark the one that does not belong. This is recognition practice, not a template to copy.",
                    QuizId = "helio",
                    Links = new[]
                    {
                        new WebLink("Northline", "northline.security")
                    }
                },
                new WebPage
                {
                    Url = "nightwire.social",
                    Title = "Nightwire — public board",
                    Body =
                        "Public board. Anyone can post. Nobody here is vetted.\n\n" +
                        "graywhale wrote:\n" +
                        "\"$NOVA is about to move. The developer is known, liquidity is deep, trust me.\"\n\n" +
                        "The account has no older posts. Read the token in the terminal with analyze nova before you act on it.",
                    Links = new[]
                    {
                        new WebLink("Daily Grid", "dailygrid.news"),
                        new WebLink("Start", "nexus.search")
                    }
                },
                new WebPage
                {
                    Url = "helio.bank/account",
                    Title = "Helio Mutual — account",
                    Body = "",
                    Links = new[]
                    {
                        new WebLink("Helio home", "helio.bank"),
                        new WebLink("Grayhaven Parts", "parts.grayhaven.shop")
                    }
                },
                new WebPage
                {
                    Url = "parts.grayhaven.shop",
                    Title = "Grayhaven Parts",
                    Body = "Used laptop parts for students. The balance is checked with Helio when you buy.",
                    Links = new[]
                    {
                        new WebLink("Helio account", "helio.bank/account"),
                        new WebLink("Start", "nexus.search")
                    }
                },
                new WebPage
                {
                    Url = "dailygrid.news",
                    Title = "Daily Grid",
                    Body = "",
                    Links = new[]
                    {
                        new WebLink("Start", "nexus.search")
                    }
                }
            };
        }

        static OsintNode[] Nodes()
        {
            return new[]
            {
                new OsintNode
                {
                    Id = "novamart",
                    Title = "NovaMart",
                    Kind = "Company",
                    Fact = "Grayhaven retailer. Public site still links a staging host.",
                    Keywords = new[] { "novamart", "company", "retailer" }
                },
                new OsintNode
                {
                    Id = "mara",
                    Title = "Mara Ellison",
                    Kind = "Employee",
                    Fact = "IT intern named on the NovaMart careers page.",
                    RequiresKnown = "novamart",
                    Keywords = new[] { "mara", "ellison", "intern" }
                },
                new OsintNode
                {
                    Id = "mara_mail",
                    Title = "m.ellison@novamart.com",
                    Kind = "Email",
                    Fact = "Mailbox printed next to Mara Ellison. The domain is NovaMart's.",
                    RequiresKnown = "mara",
                    Keywords = new[] { "m.ellison@novamart.com", "ellison@novamart.com", "mailbox", "email", "mara_mail" }
                },
                new OsintNode
                {
                    Id = "dev_site",
                    Title = "dev.novamart.com",
                    Kind = "Domain",
                    Fact = "Public staging host named on the careers page. The note says it runs ShelfStack and AuthLite.",
                    RequiresKnown = "novamart",
                    Keywords = new[] { "dev.novamart.com", "dev_site", "staging", "dev" }
                },
                new OsintNode
                {
                    Id = "shelstack",
                    Title = "ShelfStack 2.1",
                    Kind = "Technology",
                    Fact = "Fictional storefront stack named in the staging note.",
                    RequiresKnown = "dev_site",
                    Keywords = new[] { "shelstack", "shelf", "stack" }
                },
                new OsintNode
                {
                    Id = "authlite",
                    Title = "AuthLite 1.0",
                    Kind = "Weakness",
                    Fact = "Old login module named in the same public staging note. Staff already called it outdated.",
                    RequiresKnown = "dev_site",
                    Keywords = new[] { "authlite", "auth", "login" }
                }
            };
        }

        static OsintEdge[] Edges()
        {
            return new[]
            {
                new OsintEdge { A = "mara", B = "mara_mail", RequiredForFootprint = true },
                new OsintEdge { A = "mara_mail", B = "novamart", RequiredForFootprint = false },
                new OsintEdge { A = "novamart", B = "dev_site", RequiredForFootprint = true },
                new OsintEdge { A = "dev_site", B = "shelstack", RequiredForFootprint = false },
                new OsintEdge { A = "dev_site", B = "authlite", RequiredForFootprint = true },
                new OsintEdge { A = "shelstack", B = "authlite", RequiredForFootprint = false }
            };
        }

        static MissionDef[] Missions()
        {
            return new[]
            {
                new MissionDef
                {
                    Id = "campus_survey",
                    Title = "Campus portal survey",
                    Client = "Lumen University IT",
                    ListedPay = 40,
                    Summary = "Lumen IT posted a paid check of their public student page.",
                    Objectives = "Read https://www.lumen.edu/it, then run scan lumen in the terminal. Submit the survey from Work.",
                    Kind = MissionKind.TurnIn
                },
                new MissionDef
                {
                    Id = "novamart_footprint",
                    Title = "Public footprint, NovaMart",
                    Client = "Northline Security",
                    ListedPay = 80,
                    Summary = "A legal contract. Map only pages NovaMart already published.",
                    Objectives = "In Notes, connect Mara to her mailbox, NovaMart to dev.novamart.com, and that host to AuthLite. Then submit the map from Work.",
                    Requires = new[] { "campus_survey" },
                    Kind = MissionKind.TurnIn
                },
                new MissionDef
                {
                    Id = "novamart_decision",
                    Title = "The staging note",
                    Client = "NovaMart public page",
                    ListedPay = 0,
                    Summary = "Not a job. The staging page is still up, and only you decide what to do with it.",
                    Objectives = "Open https://www.novamart.com/dev-notes after the map is filed.",
                    Requires = new[] { "novamart_footprint" },
                    Kind = MissionKind.Choice
                },
                new MissionDef
                {
                    Id = "helio_awareness",
                    Title = "Helio training inbox",
                    Client = "Northline Security",
                    ListedPay = 120,
                    Summary = "Northline pays students to mark the one training message that does not belong.",
                    Objectives = "Open https://northline.security/inbox from Work and pick the odd message.",
                    Requires = new[] { "campus_survey" },
                    Kind = MissionKind.Quiz
                },
                new MissionDef
                {
                    Id = "nova_rumor",
                    Title = "Nightwire token rumor",
                    Client = "Nightwire readers",
                    ListedPay = 0,
                    Summary = "Not a job. A stranger posted about $NOVA. The reply, if you make one, happens on that board.",
                    Objectives = "Read https://nightwire.social and run analyze nova in the terminal.",
                    Requires = new[] { "novamart_footprint" },
                    Kind = MissionKind.Choice
                }
            };
        }

        static UpgradeDef[] Upgrades()
        {
            return new[]
            {
                new UpgradeDef
                {
                    Id = "ram_8",
                    Name = "Used 8 GB RAM",
                    Cost = 45,
                    Detail = "The laptop stops dying when several windows are open. Five windows at once.",
                    Purchasable = true
                },
                new UpgradeDef
                {
                    Id = "net_share",
                    Name = "Night fiber share",
                    Cost = 30,
                    Detail = "A neighbor's spare line. Pages feel less starved. Does not reach the outside world for real.",
                    Purchasable = true
                },
                new UpgradeDef
                {
                    Id = "cpu_refurb",
                    Name = "Refurbished quad-core",
                    Cost = 220,
                    Detail = "Full token readings fit in memory. Partial analysis stops dropping rows.",
                    Purchasable = true
                },
                new UpgradeDef
                {
                    Id = "workstation",
                    Name = "Custom workstation",
                    Cost = 2400,
                    Detail = "Grayhaven suppliers do not answer unknown students. Later.",
                    Purchasable = false
                }
            };
        }

        static NewsItem[] News()
        {
            return new[]
            {
                new NewsItem
                {
                    Id = "rent",
                    Headline = "Grayhaven rents jump again before semester lock.",
                    Body = "Campus boards are full of night work. Lumen has not posted emergency grants."
                },
                new NewsItem
                {
                    Id = "meridian",
                    Headline = "Meridian Digital Office drafts a student-data rule.",
                    Body = "The draft is public and unfinished. It does not name any real country."
                }
            };
        }

        static QuizDef[] Quizzes()
        {
            return new[]
            {
                new QuizDef
                {
                    Id = "helio",
                    Prompt = "Which message is the social-engineering attempt?",
                    CorrectId = "lookalike",
                    Success = "Urgency, a lookalike name, and a request for a PIN are the signals. Helio already said they do not ask for a PIN by message.",
                    Hint = "Look for a rush, a name that is almost right, and a request for a secret.",
                    Options = new[]
                    {
                        new QuizOption
                        {
                            Id = "statement",
                            Title = "Helio Mutual",
                            Body = "Your student statement is ready inside the Helio app. We never ask for a PIN by message."
                        },
                        new QuizOption
                        {
                            Id = "lookalike",
                            Title = "Helio MutuaI",
                            Body = "Unusual login. Confirm your vault PIN within 20 minutes at helio-secure.bank."
                        },
                        new QuizOption
                        {
                            Id = "library",
                            Title = "Lumen library",
                            Body = "Your reserved textbook is at the east desk until Friday."
                        }
                    }
                }
            };
        }
    }
}

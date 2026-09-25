# Tech architecture

Unity, C#. Simuliacija nepriklauso nuo Unity, kad ją būtų galima tikrinti be redaktoriaus.

## Kur kas gyvena

- `Assets/Scripts/Simulation` yra taisyklės: pinigai, heat, reputacija, misijos, OSINT, terminalas, išsaugojimas.
- `Assets/Scripts/Presentation` yra dabartinis uGUI darbalaukis.
- `WorldCatalog.cs` yra fiktyvus turinys.
- `tests/CyberUnderground.Tests` tikrina simuliaciją.

`GameSession` yra mazgas. Terminalas, naršyklė ir langai jo klausia ir gauna `StateChanged`.

## Sąsajos kelias

Dabartinis piešimas iš kodo lieka, kol veikia turinys. Naujas kiautas turi būti Unity UI Toolkit:

- UXML vietoj rankomis dėliojamų stačiakampių.
- USS kaip bendra dizaino sistema iš `ART_DIRECTION.md`.
- C# tik langų būsenai, naršyklės adresui ir simuliacijos įvykiams.

Tikro HTML variklio į projektą nededame.

## Išsaugojimas

Tekstinis failas `nexus-save.json` Unity `persistentDataPath`. Jame yra pinigai, paslėptas heat, reputacija, įgūdžiai, geležis, atliktos misijos, rasti įrašai ir ryšiai. Naujas turinys prideda lauką, o ne naują failo formatą be versijos.

## Ko nedarome vienu kartu

Generatoriaus, kripto biržos, pašto, garso ir naujo kiauto nerašome ta pačia diena. Pirmas vizualinis žingsnis yra vienas langas UI Toolkit stiliumi. Jei jis atrodo kaip OS dalis, tuo pačiu stiliumi perrašomi kiti.

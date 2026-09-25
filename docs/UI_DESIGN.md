# UI design

Pavyzdys, į kurį lygiuojamės: tamsus OS kiautas, kairė ikonų juosta, langai su antrašte ir šešėliu, naršyklė su adresu ir svetainės turiniu, terminalas su eilute `user@laptop`, kortelės, lentelės, apatinė juosta. Ne plokšti spalvoti stačiakampiai.

## Kodėl dabar atrodo kaip kvadratai

Dabartinis prototipas piešia Unity uGUI `Image` ir `Text` iš kodo. Be paveikslėlių, ikonų ir apvalių kampų tai visada bus stačiakampiai.

## HTML ir CSS

Tikro naršyklės HTML žaidimo viduje nenaudosime. Atskiros svetainės variklis pririštų žaidimą prie išorinio puslapio ir sunkiai bendrautų su C# simuliacija.

Unity turi savo porą:

- UXML yra išdėstymas, artimas HTML.
- USS yra stilių kalba, artima CSS: spalvos, tarpai, rėmeliai, `border-radius`, flex, šriftai. Tai ne visa CSS, bet užtenka OS išvaizdai.
- Perėjimai ir būsenos (`:hover`, trukmė) daromi USS ir C# klasėmis.

Kiekviena fiktyvi svetainė irgi yra UXML puslapis, ne tik teksto blokas. Adreso juosta lieka žaidimo naršyklė. Įvedus `parts.grayhaven.fake` atsidaro ta parduotuvė.

## Viena OS

Visos programos naudoja tuos pačius lango rėmus, mygtukus, korteles ir šriftą. Skiriasi tik turinys. Terminalas tamsesnis. Bankas ramesnis. Dalių parduotuvė kaip paprasta el. parduotuvė. Nė viena neturi savo atsitiktinės spalvų paletės.

## Ekrano dalys

- Kairė juosta: Guide, Browser, Terminal, Mail, Messages, Notes, Work, This PC, Settings. Aktyvi programa paryškinta.
- Darbalaukis: tamsus fonas, ne statistikos skydelis. Lipdukas gali būti, skaičių jame nėra.
- Langas: antraštė, vilkimas, minimize, maximize, close, kampo tempimas, šešėlis, 8 px kampų spindulys.
- Apatinė juosta: atidaryti langai ir laikrodis. Be eurų, heat ir reputacijos.
- Pranešimas: kortelė apačioje dešinėje, pati dingsta. Klaida ir paprastas pranešimas atrodo skirtingai.

## Būsenos, be kurių UI neatrodys gyvas

- Tuščia: paštas be laiškų, Notes be įrašų.
- Kraunasi: `Scanning...` ir juosta, ne iškart galutinis tekstas.
- Klaida: trumpas sakinys žmogaus kalba, ne stack trace.
- Užrakinta: komanda ar programa pasako, ko dar nemoki, ir neveda į realų įrankį.

## Klaviatūra

- Enter siunčia terminalo eilutę ir naršyklės adresą.
- Escape uždaro priekinį langą, jei nerašoma į lauką.
- Alt+1 … Alt+9 atidaro juostos programas.
- Ctrl+W uždaro priekinį langą.

## Ko nededame į nuolatinį HUD

Heat, reputacija ir pinigų skaičius. Pinigai yra banko puslapyje. Heat jaučiamas įvykiais.

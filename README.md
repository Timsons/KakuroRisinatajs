# KakuroRisinatajs
 Kakuro mīklas risinātājs. Kvalifikācijas darbs 2020/2021
 
 Kvalifikācijas darbs “Datora lietotne kakuro mīklu atrisināšanai” apraksta prāta spēles Kakuro
automātisko risinātāju, uzrakstītu C# valodā, kas ir pieejams kā datora lietotne. Lietotājam ir
jāievada konkrētas mīklas dati izklājlapu programmā, tādā kā MS Excel, un lietotne attēlo mīklas
atrisinājumu. Lietotne var apstrādāt arī iepriekš ierakstītas mīklas. Ievaddatu pārbaude dod
paziņojumus par iespējamo mīklas nepareizo ievadu – nav atrisinājumu vai ir vairāki
atrisinājumi. Darba gaitā izveidots Kakuro risinātājs, kas atbilst aprakstītām prasībām.

Programmas galveno funkciju - mīklas risināšanu - pilda OR-Tools algoritmi. Par OR-Tools: https://developers.google.com/optimization

Projekts glabājās mapē KakuroSolver zem nosaukuma KakuroSolver.sln. Pašrakstītais kods lielākoties ir failā KakuroSolver.cs, arī piesaistītajā projektam .csproj failā. Repozitorijā glabājas garī testpiemēri - gatavas, formatētas mīklas.

Projekta palaišanai nepieciešams uzstādīt .Net Core vismaz 3.1. versijā. Drošības pēc var uzinstalēt OR-Tools priekš .Net, lai pilnībā atkārtotu izstrādes vidi, uz kuras projekts tika veiksmīgi palaists. Instalēšanas instrukcija: https://developers.google.com/optimization/install/dotnet/windows 

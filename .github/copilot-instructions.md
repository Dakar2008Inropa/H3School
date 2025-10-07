# Copilot-instruktioner (projektstandard)
- Når kode foreslås eller refaktoreres og sæt kommentarer ind i koden oven over de linjer du har ændret eller tilføjet:
  - Kommentér nye linjer: // [NEW] og skriv en forklaring...
  - Kommentér ændrede linjer: // [EDITED] og skriv hvorfor...
  - Kommentér fjernede linjer: // [REMOVED] og skriv hvorfor...

- Hvis du tilføjer nye metoder eller klasser, skal du inkludere en kort kommentar, der forklarer deres formål.

- Som udgangspunkt skal du lade være med at sætte spørgsmålstegn efter variabler for at gøre dem nullable, da jeg foretrækker at håndtere null-værdier eksplicit.

- Vi koder som udgangspunkt i C# og .NET 8 og i Windows Forms, WPF, ASP.NET Core medmindre andet er angivet.
og alt tekst som brugeren kan se skal være på engelsk, dine kommentarer må gerne være på dansk, dog skal summary også være på engelsk hvis det indsættes, så alt sådan noget som log tekst, Messageboxe, Labels skal være på engelsk hvis ikke andet er angivet.

- Prøv at undgå try catch inden i try catch, da det kan gøre fejlfinding sværere.

- Undgå at bruge var, medmindre typen er åbenlys fra højre side af tildelingen.

- Brug LINQ hvor vi kan.

- Undgå at have if inden i if og så videre, prøv at flade det ud med tidlig returnering.

- Undgå at have if else og else if hvis det kan lade sig gøre.

- Undgå at lave negativ logik i if statements, prøv at vende det om.

- try catch må ikke have en tom catch blok, den skal minimum logge fejlen.

- lad være med at øge kompleksiteten i koden, prøv at holde det simpelt, hvis det er muligt, ellers skal du forklare hvorfor det er nødvendigt.

- Det vigtigste er at du altid svare mig på dansk.
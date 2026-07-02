# 1. Uvod

Videoigre se koriste kao eksperimentalno okruženje za istraživanje umjetne inteligencije jer omogućuju
kontrolirano, ali dovoljno kompleksno okruženje za proučavanje ponašanja autonomnih agenata [1]. Sustavi
AlphaGo [2] i OpenAI Five [3] pokazali su da agenti trenirani podržanim učenjem (engl. *reinforcement
learning*, RL) uz samostalnu igru (engl. *self-play*) razvijaju strategije bez izravnog ljudskog vodstva
ili demonstracije.

Višeagentno podržano učenje (engl. *Multi-Agent Reinforcement Learning*, MARL) proučava sustave u kojima
više agenata istovremeno uči u istom okruženju. U natjecateljskim, asimetričnim postavkama agenti imaju
suprotstavljene ciljeve. Igra lovice (engl. *tag*) jedan je od standardnih testnih zadataka takvih
sustava: jedan agent (Lovac) hvata drugog agenta (Bjegunac), koji hvatanje izbjegava. Pri jednakim
kinematičkim ograničenjima oba agenta (jednaka brzina i okretnost), problem odgovara klasičnom problemu
potjere i bijega (engl. *pursuit-evasion*) [4], u kojem hvatanje zahtijeva prostorno rasuđivanje
(presretanje, tjeranje u kut), a ne isključivo brzinu.

Problem: dodjela zasluga (engl. *credit assignment*) u timskim, natjecateljskim MARL sustavima nije
trivijalna. Kada agent dobiva nagradu tek na kraju epizode (npr. ±1 za pobjedu/poraz), potrebno je tu
nagradu ispravno rasporediti na pojedinačne akcije kroz vrijeme. Algoritam MA-POCA (*Multi-Agent
POsthumous Credit Assignment*) [5], implementiran unutar Unity ML-Agents okvira [6], rješava ovaj problem
centraliziranim kritičarom s kontrafaktualnom baznom linijom (engl. *counterfactual baseline*). Time se
razlikuje od neovisnog PPO učenja (*Proximal Policy Optimization*) [7], gdje svaki agent optimizira
isključivo vlastitu nagradu, bez uvida u stanje ili akcije drugih agenata u timu.

Drugi problem odnosi se na oblikovanje nagrade (engl. *reward shaping*). Kada je terminalna nagrada
rijetka, učenje je sporo ili se ne dogodi unutar zadanog broja koraka treniranja. Standardno rješenje je
uvođenje gustih, međukoraka nagrada koje agenta usmjeravaju prema cilju. Potencijalno oblikovanje nagrade
(engl. *potential-based shaping*, PBS) [8] teorijski jamči da takva nagrada ne mijenja optimalnu politiku
sustava. Ta garancija vrijedi za optimalnu politiku, ne i za putanju kojom je agent tijekom učenja dosegne
— razlika koja postaje značajna u nestacionarnim, samostalno-igranim okruženjima s više agenata. Ovaj rad
ispituje upravo tu razliku.

Cilj rada je utvrditi razvija li agent Lovac u kinematički simetričnoj igri lovice ponašanje potjere
isključivo na temelju rijetke, terminalne nagrade, ili je gusto oblikovanje nagrade nužan preduvjet.
Konkretni ciljevi:

1. Implementirati dvotimsko okruženje igre lovice u Unity ML-Agents okviru, u kojem agenti Lovac i
   Bjegunac uče algoritmom MA-POCA kroz samostalnu igru.
2. Potvrditi da implementirani sustav provodi MA-POCA dodjelu zasluga (a ne neovisno PPO učenje),
   analizom pokazatelja treninga (gubitak bazne linije, engl. *BaselineLoss*).
3. Provesti kontrolirani eksperiment koji uspoređuje rijetku terminalnu nagradu naspram guste PBS
   nagrade, uz identičnu arhitekturu mreže, opažajni prostor, kinematiku agenata i sjeme slučajnosti.
4. Usporediti rezultate oba režima na kratkom (400 000 koraka) i dugom (5 000 000 koraka) horizontu
   treniranja, koristeći ELO ocjenu, stopu uspješnog hvatanja, prosječno vrijeme do hvatanja i
   kumulativnu timsku nagradu.

Rad je podijeljen na sedam poglavlja. U uvodnom poglavlju opisan je problem dodjele zasluga i oblikovanja
nagrade u MARL sustavima te su definirani ciljevi rada. U drugom poglavlju dan je pregled srodnih radova
iz područja podržanog učenja, samostalne igre i oblikovanja nagrade. Treće poglavlje opisuje metodologiju:
dizajn okruženja, arhitekturu agenata, algoritam MA-POCA i dizajn eksperimenta. Četvrto poglavlje opisuje
implementaciju — programsku arhitekturu, ključne klase, konfiguraciju treniranja i reproducibilnost. Peto
poglavlje prikazuje rezultate provedenih eksperimenata. U šestom poglavlju rezultati su raspravljeni u
odnosu na istraživačko pitanje, uključujući ograničenja rada. Zaključne misli i smjernice za buduća
istraživanja dane su u sedmom, završnom poglavlju.

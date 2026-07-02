# 3. Metodologija

## 3.1 Programsko okruženje i implementacija

Okruženje je implementirano u Unity game engineu (verzija 6000.4.0f1) korištenjem Unity ML-Agents
paketa (Release 23) [6]. Igra lovice implementirana je kao asimetrična, dvotimska igra: Lovac
(*Behavior Name* `Chaser`, tim 0) progoni Bjegunca (*Behavior Name* `Runner`, tim 1). Svaka uloga
predstavlja zaseban ML-Agents *behavior* i zaseban `SimpleMultiAgentGroup`. Odvojena definicija
grupa po timu preduvjet je za MA-POCA algoritam [5]: timski ishodi dodjeljuju se pomoću
`AddGroupReward`, a epizoda završava na razini grupe (`EndGroupEpisode` pri hvatanju,
`GroupEpisodeInterrupted` pri isteku vremena).

Arena je ograničen kvadratni prostor. Oba agenta imaju identičnu kinematiku (`moveSpeed = 5`,
`turnSpeed = 180`), čime je uklonjena mogućnost da razlika u brzini objasni ishod eksperimenta.
Epizoda traje najviše 400 odluka (2000 fizikalnih koraka); odluka se donosi svakih 5 fizikalnih
koraka (`DecisionRequester period = 5`).

## 3.2 Prostor opažanja i akcija

Prostor opažanja sastoji se od 18 vrijednosti: vlastita pozicija, brzina i orijentacija agenta te
relativna pozicija, brzina i orijentacija protivnika, uz `RayPerceptionSensor3D` koji detektira
zidove arene i drugog agenta. Prostor akcija je kontinuiran, s dvije komponente: kretanje naprijed
i skretanje.

## 3.3 Algoritam MA-POCA i samostalna igra

MA-POCA (*Multi-Agent POsthumous Credit Assignment*) [5] trenira centraliziranog kritičara s
kontrafaktualnom baznom linijom (engl. *counterfactual baseline*), koja timski povrat raspoređuje
na pojedinačne agente. Ovaj mehanizam omogućuje ispravnu dodjelu zasluga i kada agent napusti
epizodu prije njezinog završetka ("posmrtna" dodjela zasluga), za razliku od neovisnog PPO učenja
[7], koje takav mehanizam nema.

Treniranje koristi samostalnu igru (engl. *self-play*): jedan tim trenira dok je drugi zamrznuta,
prethodno spremljena inačica politike (engl. *snapshot*), s periodičnom izmjenom uloga
(`team_change`). Relativna jačina politika prati se ELO ocjenom, inicijaliziranom na 1200 za oba
tima.

## 3.4 Dizajn nagrade i potencijalno oblikovanje

Terminalna nagrada iznosi ±1 pri kraju epizode (pobjeda/poraz), uz manji vremenski bonus. Lovac
dodatno prima kaznu od −0,001 po koraku, koja stvara pritisak za brzo djelovanje, ali ne pruža
prostorni gradijent prema Bjegunacu.

Za potrebe eksperimenta uveden je dodatni, gusti član nagrade za Lovca temeljen na potencijalnom
oblikovanju nagrade (engl. *potential-based shaping*, PBS) [8]. Definirana je potencijalna funkcija
`Φ(s) = −coef · (d / d_max)`, gdje je `d` planarna udaljenost između Lovca i Bjegunca, a `d_max ≈ 28`
dijagonala je arene. Dodatna nagrada u svakom koraku iznosi `F = γ·Φ(s′) − Φ(s)`, s `γ = 0,99`
(faktor diskontiranja trenera), a `coef = 0,5`. PBS je odabran jer teorijski jamči da dodana nagrada
ne mijenja optimalnu politiku podležećeg MDP-a [8] — jamstvo koje vrijedi za optimalnu politiku, ne
i za putanju učenja, posebice pod `γ < 1` i u nestacionarnom, samostalno-igranom okruženju s više
agenata.

## 3.5 Dizajn eksperimenta

Eksperiment uspoređuje dva režima (engl. *arms*), identična u svemu osim vrijednosti `coef`:

| | Rijetka nagrada | Gusta (PBS) nagrada |
|---|---|---|
| Terminalna nagrada (±1 + bonus) | da | da |
| Kazna −0,001/korak (Lovac) | da | da |
| PBS oblikovanje udaljenosti (Lovac) | isključeno (`coef = 0`) | uključeno (`coef = 0,5`) |
| Kinematika (Lovac/Bjegunac) | 5 / 5 | 5 / 5 |
| Opažanja, arhitektura mreže, samostalna igra, sjeme | identično | identično |

Svaki režim pokrenut je s 3 različita sjemena slučajnosti radi provjere varijance rezultata.
Treniranje je provedeno na dva vremenska horizonta: kratki (400 000 koraka po ponašanju, 8 arena) i
dugi (5 000 000 koraka po ponašanju, 16 arena, headless build bez renderiranja). Odabir režima
proveden je isključivo preko konfiguracije trenera
(`environment_parameters.distance_shaping_coef`), a konfiguracije obaju režima arhivirane su u
`experiments/configs/`.

Unaprijed definirano pravilo uspjeha (pre-registrirano prije pokretanja treninga): režim se smatra
uspješnim ako do 400 000 koraka stopa uspješnog hvatanja premaši početnih ~15%, prosječna duljina
epizode padne ispod ~393 koraka odluke, i ELO ocjena divergira od 1200 u suprotnim smjerovima za oba
tima.

## 3.6 Sklopovlje i paralelizacija

Treniranje je provedeno na prijenosnom računalu s procesorom Intel i7-9750H (6 jezgri / 12 dretvi),
16 GB RAM-a i GPU-om NVIDIA GTX 1660 Ti (4 GB). Analiza vremenske raspodjele (`timers.json`) pokazala
je da je opterećenje dominantno vezano za simulaciju okruženja i međuprocesnu komunikaciju
(Unity↔Python), a ne za neuronsku mrežu: obrada gradijenata čini svega ~6% ukupnog vremena. GPU stoga
ima zanemariv utjecaj na brzinu treniranja pri ovoj veličini mreže (256×2); dominantna poluga za
ubrzanje treniranja je broj paralelnih arena.

## 3.7 Metrike evaluacije

Za usporedbu režima korištene su sljedeće metrike, zabilježene tijekom treniranja:

- ELO ocjena po timu (`Self-play/ELO`) — mjera relativne kompetitivne prednosti.
- Kumulativna timska nagrada (`Environment/GroupCumulativeReward`) — stvarni ishod igre, neovisan o
  dodatnom PBS članu nagrade.
- Stopa uspješnog hvatanja (`Environment/Catch`) i prosječno vrijeme do hvatanja
  (`Environment/TimeToCatch`), zabilježeni prilagođenim `StatsRecorder` pozivima. U pokretanjima na
  400 000 koraka `TimeToCatch` je zbog pogreške u redoslijedu bilježenja (vrijednost se bilježila
  nakon poziva koji resetira brojač koraka) sustavno iznosio nula; pogreška je otklonjena prije
  pokretanja treninga na 5 000 000 koraka. Za rezultate na kratkom horizontu (§5.3) stoga je kao
  zamjenska mjera brzine hvatanja korištena prosječna duljina epizode.
- Prosječna duljina epizode (`Environment/EpisodeLength`) — posredna mjera vještine Lovca, korištena
  i kao zamjena za `TimeToCatch` gdje potonji nije bio dostupan (vidi gore).
- Gubici treniranja (`Losses/PolicyLoss`, `Losses/ValueLoss`, `Losses/BaselineLoss`) — potonji služi i
  kao izravan dokaz da implementirani sustav provodi MA-POCA dodjelu zasluga, budući da se u
  neovisnom PPO učenju ovaj gubitak ne pojavljuje.

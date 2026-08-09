# Nacrt teksta za dva nova potpoglavlja

> **Kako koristiti:** tekst je pisan u stilu ostatka rada (hrvatski, `eng. „…"` za nazivlje,
> brojevi s decimalnim zarezom). Mjesta označena `⟨…⟩` moraju se popuniti iz tvoje YAML
> konfiguracije — nisu poznata iz `Theory.md` i **ne smiju se izmišljati**.
> Jednadžbe su zapisane tekstualno; u Wordu ih upiši kroz *Insert → Equation* radi
> dosljednosti s postojećim jednadžbama u radu.

---

# 3.5 Samoiga (eng. „Self-play") i ELO ocjenjivanje

*(Novo potpoglavlje, dolazi nakon 3.4 MA-POCA, prije poglavlja 4.)*

## Uvodni tekst potpoglavlja

Algoritmi opisani u prethodnim potpoglavljima definiraju **kako** agent ažurira svoju politiku,
ali ne i **protiv koga** uči. U kompetitivnom višeagentnom okruženju taj je izbor jednako
važan kao i sam optimizacijski postupak. Ako se agent trenira protiv protivnika fiksne,
unaprijed programirane strategije, naučena politika bit će optimalna samo protiv te jedne
strategije i tipično neće generalizirati. S druge strane, ako oba agenta uče istovremeno i
neograničeno, okolina koju svaki od njih percipira postaje izrazito nestacionarna — problem
opisan u potpoglavlju 3.3 — pa se učenje može zarobiti u cikličkom obrascu u kojem se politike
međusobno „love" bez trajnog napretka.

Samoiga (eng. „self-play") jest postupak treniranja u kojem agent uči igrajući protiv **vlastitih
ranijih inačica**. Time se istovremeno rješavaju oba problema: protivnik nikada nije trivijalan
jer je i sam proizvod istog procesa učenja, a nestacionarnost se kontrolira jer se protivnička
politika mijenja u diskretnim, upravljanim koracima umjesto kontinuirano. Upravo je samoiga
mehanizam koji stoji iza najpoznatijih dostignuća spomenutih u uvodnom poglavlju — AlphaGo Zero
i OpenAI Five — a u ovom radu ona je nužan uvjet za pojavu emergentnog ponašanja jer bez nje
niti jedan od dvaju agenata ne bi imao protivnika koji raste zajedno s njim.

### 3.5.1 Prirodni kurikulum

Ključno svojstvo samoigre u kompetitivnom okruženju jest da ona stvara **prirodni kurikulum**
(eng. „natural curriculum"), pojam uveden u potpoglavlju 2.3.1. Težina zadatka nije zadana
izvana, nego proizlazi iz trenutne sposobnosti protivnika: kada Chaser postane bolji u progonu,
Runner je prisiljen razviti sofisticiraniju strategiju bijega, što zauzvrat postavlja teži
zadatak pred Chasera. Nastaje takozvana trka u naoružanju (eng. „arms race") u kojoj se razina
igre podiže postupno i samostalno, bez ručno definiranog rasporeda težine.

Ova je dinamika izravno relevantna za središnje istraživačko pitanje rada. Emergentno ponašanje
progona i bijega nije programirano niti nagrađeno posebnim signalom — ono nastaje kao odgovor na
protivnika koji se i sam usavršava. U okruženju s fiksnim protivnikom takva se pojava ne bi
mogla ni očekivati ni izmjeriti.

### 3.5.2 Mehanizam snimaka politike

Unity ML-Agents implementira samoigru pohranjivanjem **snimaka** (eng. „snapshots") politike u
pravilnim razmacima tijekom treniranja. U svakom trenutku samo jedan tim aktivno uči, dok drugi
tim — nazvan protivničkim ili „duh" timom (eng. „ghost team") — djeluje prema jednoj od
pohranjenih, zamrznutih inačica politike. Postupak je upravljan sljedećim parametrima:

- **`save_steps`** — broj koraka treniranja između dvaju uzastopnih spremanja snimke politike.
  Manja vrijednost daje gušći niz protivnika, ali i međusobno sličnije protivnike.
- **`swap_steps`** — broj koraka nakon kojih se protivniku zamjenjuje snimka, čime se sprječava
  prilagodba (eng. „overfitting") na jednog određenog protivnika.
- **`window`** — veličina kliznog prozora pohranjenih snimaka iz kojeg se protivnik uzorkuje.
  Veći prozor znači raznolikiji, ali u prosjeku slabiji skup protivnika.
- **`play_against_latest_model_ratio`** — vjerojatnost da će protivnik biti *najnovija* politika,
  a ne nasumično odabrana starija snimka. Viša vrijednost ubrzava trku u naoružanju, ali povećava
  rizik od cikličkog ponašanja i zaboravljanja ranije naučenih strategija.
- **`team_change`** — broj koraka nakon kojih uloge učenja zamjenjuju mjesta: tim koji je učio
  postaje zamrznuti protivnik, a dosadašnji protivnik počinje učiti.

U konfiguraciji korištenoj u ovom radu te vrijednosti iznose ⟨`save_steps`⟩, ⟨`swap_steps`⟩,
⟨`window`⟩, ⟨`play_against_latest_model_ratio`⟩ i ⟨`team_change`⟩, a cjelovita konfiguracija
navedena je u potpoglavlju 4.6.

Važno je istaknuti jednu posljedicu ovog mehanizma koja se pri prvom susretu lako pogrešno
protumači kao greška. Budući da zamrznuti tim i dalje koraca kroz okolinu kao protivnik, iako
se njegovi koraci ne koriste za ažuriranje politike, ukupni **brojač koraka agenta premašuje
nominalni budžet treniranja**. U provedenom dim-testu s nominalnih 50 000 koraka Chaser je
zabilježio 60 267 koraka. To je normalno računovodstvo samoigre, a ne pogreška: budžet se
odnosi na korake učenja pojedinog ponašanja, ne na ukupan broj simuliranih koraka.

### 3.5.3 ELO ocjenjivanje

Za mjerenje relativne uspješnosti dvaju timova tijekom treniranja koristi se ELO sustav
ocjenjivanja, izvorno razvijen za rangiranje šahista. Svakom se timu pridružuje brojčana ocjena
koja se ažurira nakon svake odigrane epizode ovisno o ishodu i o očekivanju koje je ocjena prije
epizode implicirala.

Očekivani rezultat tima A protiv tima B definiran je izrazom:

    E_A = 1 / (1 + 10^((R_B − R_A) / 400))

gdje su `R_A` i `R_B` trenutne ELO ocjene timova. Nakon odigrane epizode ocjena se ažurira kao:

    R_A ← R_A + K · (S_A − E_A)

pri čemu je `S_A` stvarni ishod (1 za pobjedu, 0 za poraz), a `K` konstanta koja određuje
osjetljivost ocjene na pojedinačni rezultat. Oba tima kreću od početne vrijednosti od
**1200 bodova**, pa je **divergencija od 1200 u suprotnim smjerovima** izravan pokazatelj da je
jedna uloga stekla prednost nad drugom.

Pri tumačenju ELO ocjena u ovom radu nužne su dvije ograde. Prvo, budući da su uloge Chasera i
Runnera **asimetrične i nikada se ne zamjenjuju**, ELO ovdje ne mjeri apsolutnu vještinu agenta
nego **ravnotežu između dviju uloga**: ocjena Chasera od 1890 nasuprot Runnerovih 665 ne znači
da je Chaser „dobar igrač" u apsolutnom smislu, nego da u toj konfiguraciji uloga Chasera
dominira. Drugo, ELO je **interno kalibriran unutar jednog pokretanja** — vrijednosti iz dvaju
različitih pokretanja nisu izravno usporedive jer su ocjene nastale u odvojenim populacijama
protivnika. Zbog toga se u poglavlju 6 ELO koristi kao pokazatelj *smjera i veličine razdvajanja*
unutar pojedinog eksperimenta, dok se usporedbe između eksperimenata oslanjaju na stopu hvatanja
i duljinu epizode.

### 3.5.4 Validacija mehanizma

Ispravnost implementacije samoigre potvrđena je prije glavnih pokretanja. U dim-testu ELO ocjene
razdvojile su se od početnih 1200 na 1206,4 (Chaser) i 1195,1 (Runner) — razlika od približno 11
bodova sama po sebi jest šum na tako kratkom horizontu, ali potvrđuje da je petlja ocjenjivanja
aktivna. Mehanizam `team_change` uredno se aktivirao, što je u konzoli vidljivo kao prelazak
Chasera u stanje „Not Training" uz istovremeni nastavak učenja Runnera. Snimke politike spremane
su na zadanom intervalu, a konačni modeli izvezeni su u `.onnx` format uz uredno zaustavljanje
procesa.

---

# 5.2 Mjerne veličine i kriteriji vrednovanja

*(Novo potpoglavlje, dolazi nakon 5.1 „Istraživačka pitanja i hipoteze", a **prije** opisa
eksperimenata — jer kriteriji uspjeha u 5.2.1 već koriste ove veličine.)*

## Uvodni tekst potpoglavlja

Prije opisa pojedinačnih eksperimenata potrebno je precizno definirati veličine kojima se mjeri
uspješnost treniranja. Ovaj korak nije formalnost: u eksperimentu u kojem se uspoređuju dvije
ruke treniranja s **različitim funkcijama nagrade**, pogrešan odabir mjerne veličine vodi do
suprotnog zaključka od ispravnog. Konkretno, ruka s oblikovanjem nagrade po definiciji prikuplja
veću ukupnu nagradu od ruke bez oblikovanja, neovisno o tome igra li išta bolje — pa usporedba
po ukupnoj nagradi nije valjana. Zbog toga se u nastavku mjerne veličine dijele u tri skupine i
eksplicitno se navodi koje se smiju koristiti za usporedbu između ruku.

Sve navedene veličine bilježe se tijekom treniranja i vizualiziraju alatom TensorBoard. Dvije
veličine specifične za ovo istraživanje — stopa hvatanja i vrijeme do hvatanja — nisu dio
standardnog skupa koji ML-Agents emitira, nego su dodane vlastitom implementacijom putem
`StatsRecorder` sučelja unutar razreda `TagArenaManager`.

### 5.2.1 Ishodne metrike

Ishodne metrike opisuju **stvarni rezultat igre** i neovisne su o tome kako je nagrada oblikovana.

**Stopa hvatanja** (`Environment/Catch`). Pri završetku svake epizode bilježi se vrijednost 1 ako
je epizoda završila hvatanjem, odnosno 0 ako je završila istekom vremena. Prosjek te veličine
kroz promatrani interval izravno daje udio epizoda u kojima je Chaser uspio — što je
najinterpretabilnija pojedinačna mjera uspješnosti progona u ovom radu. Vrijednost blizu 0
označava potpunu dominaciju Runnera, a vrijednost blizu 1 dominaciju Chasera.

**Duljina epizode** (`Environment/Episode Length`). Prosječan broj koraka odlučivanja po epizodi,
usrednjen kroz **sve** epizode. Budući da epizoda traje najviše 400 koraka odlučivanja,
vrijednost blizu te granice znači da većina epizoda završava istekom vremena. Kod agenta koji
uči progon ova veličina mora **padati** tijekom treniranja, pa služi kao neizravna mjera
učinkovitosti progona (eng. „time-to-catch proxy").

**Vrijeme do hvatanja** (`Environment/TimeToCatch`). Broj fizikalnih koraka arene u trenutku
hvatanja, usrednjen **isključivo kroz epizode koje su završile hvatanjem**. Za razliku od duljine
epizode, ova veličina ne miješa uspješne i neuspješne epizode, pa mjeri koliko je progon
*učinkovit* kada uspije, neovisno o tome koliko često uspijeva.

> **Napomena o odnosu dviju veličina.** Duljina epizode i vrijeme do hvatanja mjere **različite
> populacije epizoda** (sve epizode nasuprot samo uspješnima) i izražene su u različitim
> jedinicama (koraci odlučivanja nasuprot fizikalnim koracima). Nisu međusobno pretvorive
> jednostavnim množenjem s periodom odlučivanja od 5, i moraju se tumačiti odvojeno.

**Grupna kumulativna nagrada** (`Environment/Group Cumulative Reward`). Zbroj nagrada isporučenih
grupnim kanalom, dakle terminalni ishod ±1 uvećan za vremenski bonus odnosno bonus preživljavanja.
Budući da oblikovanje nagrade ulazi isključivo u individualni kanal, ova veličina **ostaje
identično definirana u obje ruke eksperimenta** i stoga predstavlja najčišći pokazatelj stvarnog
ishoda igre. Vrijednost −1 znači da Chaser gubi svaku epizodu; vrijednost iznad +1 znači da ne
samo da hvata, nego hvata brzo i skuplja znatan vremenski bonus.

### 5.2.2 Kompetitivne metrike

**ELO ocjena** (`Self-play/ELO`). Opisana u potpoglavlju 3.5.3. Prati se divergencija obiju ocjena
od početne vrijednosti 1200; razlika između ocjena Chasera i Runnera koristi se kao mjera
kompetitivnog razdvajanja. ELO je neovisan o obliku funkcije nagrade jer se računa isključivo iz
ishoda epizoda, što ga čini valjanim za usporedbu ruku, ali je relativan unutar pokretanja pa
nije kalibriran između različitih eksperimenata.

### 5.2.3 Dijagnostičke metrike

Dijagnostičke metrike ne mjere uspješnost u igri, nego zdravlje samog procesa učenja.

**Entropija politike** (`Policy/Entropy`) mjeri nasumičnost odabira akcija. Visoka vrijednost
označava istraživanje (eng. „exploration") i očekuje se u ranim fazama treniranja; pad entropije
označava prelazak u iskorištavanje (eng. „exploitation") i izoštravanje politike. Entropija koja
ostaje visoka nakon dugog treniranja upućuje na to da politika nije konvergirala.

**Gubici** (`Losses/Policy Loss`, `Losses/Value Loss`, `Losses/Baseline Loss`) prate stabilnost
optimizacije. Posebnu ulogu ima `Baseline Loss`: taj član odgovara treniranju kontrafaktične
bazne mreže i **postoji isključivo u MA-POCA algoritmu**. Njegova prisutnost stoga služi kao
izravan dokaz da je korišten MA-POCA trener, a njegova odsutnost u PPO pokretanjima kao dokaz
suprotnog — argument razrađen u potpoglavlju 6.1.

**Procjene vrijednosti** (`Policy/Extrinsic Value Estimate`, `Policy/Extrinsic Baseline Estimate`)
pokazuju što kritičar predviđa o ishodu epizode. Suprotni predznaci za dva tima potvrđuju da
kritičar ispravno prepoznaje tko je u prednosti.

**Individualna kumulativna nagrada** (`Environment/Cumulative Reward`) zbroj je svih nagrada
isporučenih individualnim kanalom, uključujući vremenski pritisak i, u oblikovanoj ruci, PBS
oblikovanje. Ova se veličina **ne smije koristiti za usporedbu između ruku** (v. 5.2.4), ali je
dragocjena kao dijagnostika: velik razmak između individualne i grupne nagrade izravan je
pokazatelj da agent prikuplja nagradu iz oblikovanja, a ne iz stvarnog ishoda igre — što je
mehanizam otkriven u potpoglavlju 6.3.

### 5.2.4 Pravilo usporedbe između ruku treniranja

Iz prethodnog slijedi pravilo koje se dosljedno primjenjuje u cijelom poglavlju 6:

> **Usporedbe između rijetke i oblikovane ruke provode se isključivo pomoću metrika neovisnih o
> oblikovanju: stope hvatanja, duljine epizode, vremena do hvatanja, grupne kumulativne nagrade i
> ELO ocjene. Individualna kumulativna nagrada koristi se samo unutar jedne ruke, kao
> dijagnostika.**

Razlog je jednostavan: oblikovana ruka prima dodatni signal nagrade kojeg rijetka ruka uopće
nema, pa je njezina individualna nagrada veća po konstrukciji, a ne po uspješnosti. Zanemarivanje
ovog pravila vodi do zaključka koji je upravo suprotan stvarnom stanju — što je, kako pokazuje
poglavlje 6.3, najvažniji pojedinačni nalaz ovog rada.

### Tablica 5-1 Pregled mjernih veličina

| Veličina (TensorBoard) | Skupina | Jedinica / raspon | Očekivano pri učenju | Usporediva između ruku |
|---|---|---|---|---|
| `Environment/Catch` | ishodna | 0–1 | ↑ | **DA** |
| `Environment/Episode Length` | ishodna | koraci odlučivanja, ≤ 400 | ↓ | **DA** |
| `Environment/TimeToCatch` | ishodna | fizikalni koraci | ↓ | **DA** |
| `Environment/Group Cumulative Reward` | ishodna | ≈ −1 do > +1 | ↑ (Chaser) | **DA** |
| `Self-play/ELO` | kompetitivna | od 1200, relativno | divergencija | **DA** (unutar pokretanja) |
| `Environment/Cumulative Reward` | dijagnostička | ovisi o oblikovanju | — | **NE** |
| `Policy/Entropy` | dijagnostička | > 0 | ↓ | ne primjenjivo |
| `Losses/Baseline Loss` | dijagnostička | > 0 | konačna vrijednost | ne primjenjivo |

*(Preimenuj oznaku tablice prema numeraciji rada — ako Tablica 3 već postoji u poglavlju 6,
ova postaje **Tablica 3**, a postojeće se pomiču.)*

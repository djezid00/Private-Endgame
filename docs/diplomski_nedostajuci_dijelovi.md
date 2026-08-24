# Diplomski rad — nedostajući dijelovi (hrvatski tekst za unos u .docx)

> **Napomena o datoteci.** Ovaj dokument sadrži tekst koji u `.docx` datoteci rada još ne postoji ili
> je u njoj pogrešan. Datoteka
> `ANALIZA KOMPETITIVNE INTERAKCIJE U VIDEO IGRAMA POMOĆU MULTI-AGENTSKOG STROJNOG UČENJA.docx`
> **nije mijenjana** — svi se odlomci ovdje pripremaju za ručno kopiranje u Word.
>
> Mjesta slika i tablica označena su blokovima `[SLIKA — …]` odnosno `[TABLICA — …]`. Svaki blok
> sadrži naziv izvorne datoteke grafa, TensorBoard oznaku koju treba prikazati i predloženi
> potpis u stilu ostatka rada.

---

## Pregled: što nedostaje i kamo ide

| # | Dio rada | Stanje u `.docx` | Radnja |
|---|---|---|---|
| 1 | 1 UVOD — pregled poglavlja | **pogrešan tekst** (opisuje Apache Kafka / WebSocket / Taxi aplikaciju iz drugog rada) | zamijeniti odlomcima iz § 1 ovog dokumenta |
| 2 | 5.1 Istraživačka pitanja | nedostaje RQ-D | dodati odlomak iz § 2 |
| 3 | 5.8 Faza C (novo potpoglavlje) | ne postoji | umetnuti § 3 iza „Faza B: nasumičan raspored prepreka" |
| 4 | 6.1 Dokaz putem Baseline Loss | postoji, ali zaključak treba pooštriti | dodati odlomke iz § 4 na kraj potpoglavlja |
| 5 | 6.7 Rezultati Faze C (novo) | ne postoji | umetnuti § 5 iza potpoglavlja 6.6 |
| 6 | 6.8 Rasprava — Faza C (novo) | ne postoji | umetnuti § 6 |
| 7 | ZAKLJUČAK | **prazan** (samo naslov) | umetnuti § 7 |
| 8 | SAŽETAK / ABSTRACT | **prazan** (samo oznake polja) | umetnuti § 8 |
| 9 | Popis oznaka i kratica | nepotpun | dodati stavke iz § 9 |
| 10 | Kazalo slika / tablica | nedostaju stavke Faze C | Word ih generira automatski nakon umetanja potpisa; popis u § 10 služi kao kontrola |
| 11 | Ostali prilozi i dokumentacija | **prazan** | umetnuti § 11 |

---

## § 1 — UVOD: zamjena pregleda poglavlja

> **Zamjenjuje** postojeće odlomke koji počinju s „Rad je podijeljen na sedam poglavlja. U uvodnom
> poglavlju objašnjena je potreba za inovativnim pristupima u upravljanju komunikacijom između
> mikroservisa…" i završavaju s „…Zaključne misli donesene su u istoimenom, završnom poglavlju."
> Ti su odlomci preuzeti iz drugoga rada i ne odnose se na ovu temu.

Rad je podijeljen na sedam poglavlja. U uvodnom je poglavlju obrazložen kontekst istraživanja:
uloga video igara kao kontroliranog poligona za razvoj algoritama umjetne inteligencije, niz
prekretnica koje su pojačano strojno učenje uvele u širu javnost te formulacija središnjeg
istraživačkog pitanja o mogućnosti razvoja emergentnih strategija progona i bijega bez eksplicitnog
programiranja tih ponašanja.

Drugo poglavlje donosi teorijsku podlogu pojačanog strojnog učenja. Objašnjeni su temeljni pojmovi
— agent, okolina, nagrada, model, politika i vrijednosna funkcija — te formalni okvir Markovljeva
procesa odlučivanja i njegovo proširenje na Markovljeve (stohastičke) igre, koje je nužno jer u
kompetitivnoj igri okolina za svakog agenta uključuje i protivnika koji istodobno uči.

Treće poglavlje daje pregled korištenih algoritama. Polazi od algoritma proksimalne optimizacije
politike (PPO) kao osnovice, potom uvodi problematiku višeagentnog učenja i paradigmu
centraliziranog treniranja uz decentralizirano izvođenje, te detaljno razrađuje algoritam MA-POCA:
problem dodjele zasluga, nedostatke klasičnog rješenja apsorbirajućim stanjima, arhitekturu
kritičara sa samopažnjom i oblik vrijednosne funkcije. Poglavlje završava prikazom oblikovanja
nagrade te mehanizama samoigre i ELO ocjenjivanja.

Četvrto poglavlje opisuje okruženje i implementaciju. Predstavljen je okvir Unity ML-Agents i
međuprocesna komunikacija između Unity simulacije i Python trenera, zatim pravila same igre Lovice
kao asimetrične kompetitivne igre, prostor opažanja i prostor akcija, struktura nagrada i grupnih
epizoda te konfiguracija treniranja s paralelizacijom kroz šesnaest istodobnih arena. Posebno je
analizirano uočeno usko grlo u propusnosti treniranja.

Peto poglavlje razrađuje dizajn eksperimenta. Postavljena su istraživačka pitanja i hipoteze,
precizno su definirane mjerne veličine i podijeljene na ishodne, kompetitivne i dijagnostičke, te je
uvedeno pravilo koje određuje kojim se veličinama smije uspoređivati različite ruke treniranja.
Nakon toga slijedi opis četiriju provedenih eksperimenata: usporedba rijetke i oblikovane nagrade,
usporedba algoritama MA-POCA i PPO, pretraživanje diskontnog faktora γ u arenama s preprekama
(faze A i B) te završni višeagentni eksperiment u postavi dva na dva (Faza C).

Šesto poglavlje iznosi empirijske rezultate svih provedenih pokusa, uključujući verifikaciju da je
korišten upravo MA-POCA trener, inverziju ranga između validacijskog pokusa na 400 000 koraka i
glavnih pokusa na 5 · 10⁶ koraka, mehanizam parazitskog skupljanja nagrade pri oblikovanoj nagradi,
osjetljivost učenja na diskontni faktor te rezultate u arenama s fiksnim i nasumičnim rasporedom
prepreka. Poglavlje završava rezultatima i raspravom višeagentne faze, u kojoj se prvi put mjeri
razlika između algoritama MA-POCA i PPO u uvjetima u kojima je ona uopće moguća.

Zaključne misli, ograničenja provedenog istraživanja i smjernice za budući rad doneseni su u
istoimenom, završnom poglavlju.

---

## § 2 — Dopuna potpoglavlja 5.1 (Istraživačka pitanja i hipoteze)

> **Umetnuti** iza postojećeg popisa RQ-A, RQ-B i RQ-C.

RQ-D: Ostvaruje li algoritam MA-POCA mjerljivu prednost pred algoritmom PPO kada veličina grupe
prijeđe jedan, odnosno kada kontrafaktična bazna mreža uopće ima suigrače o kojima može zaključivati?

Ovo je pitanje postavljeno naknadno, nakon što je analiza rezultata Eksperimenta 2 pokazala da
usporedba provedena u postavi jedan na jedan ne može razlikovati ta dva algoritma iz načelnih, a ne
mjernih razloga. Obrazloženje i formalni izvod nalaze se u potpoglavlju 6.1, a sam je eksperiment
opisan u potpoglavlju 5.8 i njegovi rezultati u potpoglavlju 6.7.

---

## § 3 — Novo potpoglavlje 5.8: Faza C — višeagentni eksperiment (2v2)

> **Umetnuti** kao novo potpoglavlje razine 2, iza „Faza B: nasumičan raspored prepreka".

### 5.8 Faza C: višeagentni eksperiment (dva na dva)

Sve dosad opisane konfiguracije provedene su u postavi jedan na jedan, s po jednim lovcem i jednim
bjeguncem. Takva postava ima jedno svojstvo koje je za ovaj rad ključno, a koje je uočeno tek pri
analizi rezultata: pri veličini grupe jedan kontrafaktična bazna mreža algoritma MA-POCA nema o čemu
zaključivati, pa se svodi na običnu vrijednosnu funkciju. Posljedica je da se algoritmi MA-POCA i
PPO u toj postavi razlikuju samo u jednom koeficijentu funkcije gubitka, a ne u načinu dodjele
zasluga. Detaljan izvod te tvrdnje i njezina mjerna potvrda izneseni su u potpoglavlju 6.1.

Iz toga slijedi da nijedan rezultat prethodnih eksperimenata ne može odgovoriti na pitanje zbog
kojega je MA-POCA i odabran kao predmet rada. Svrha je Faze C stvoriti uvjete u kojima je razlika
između tih dvaju algoritama uopće moguća, a to znači uvesti timove.

**Postava.** Arena sadrži dva lovca i dva bjegunca, pri čemu svaka uloga čini vlastitu grupu
(`SimpleMultiAgentGroup`). Ostali su parametri zadržani na radnoj točki utvrđenoj u prethodnim
fazama: rijetka nagrada, γ = 0,99, četiri nasumično raspoređena stupa, šesnaest paralelnih arena i
5 · 10⁶ koraka po pokretanju.

**Posmrtna dodjela zasluga.** Uhvaćeni se bjegunac deaktivira, ali epizoda **ne završava** — traje
sve dok i drugi bjegunac ne bude uhvaćen ili dok ne istekne vrijeme. Time se prvi put u ovom radu
aktivira upravo ono svojstvo po kojemu je algoritam dobio ime: agent koji je napustio epizodu i dalje
mora primiti zasluge za ishod na koji je utjecao dok je bio prisutan.

**Opažanja.** Vlastito stanje agenta ostaje u vektorskom osjetilu i broji istih osamnaest realnih
brojeva kao u postavi jedan na jedan, čime je osigurano da se opažanja u kontrolnim uvjetima nisu
promijenila. Svi ostali agenti u areni ulaze u međuspremničko osjetilo (`BufferSensorComponent`), po
deset brojeva za svakog agenta i najviše sedam agenata. To je osjetilo permutacijski invarijantno —
poredak suigrača i protivnika ne utječe na izlaz — pa jedna te ista specifikacija ponašanja vrijedi
za bilo koju veličinu tima, bez ponovnog treniranja arhitekture.

**Ruke treniranja.** Uspoređuju se dvije ruke koje dijele sve hiperparametre; razlikuju se isključivo
u vrsti trenera i u kanalu kojim se isporučuje terminalna nagrada:

[TABLICA — Tablica 5.3 Dizajn ruku treniranja u Fazi C]

| Parametar | Ruka MA-POCA | Ruka PPO |
|---|---|---|
| Trener (`trainer_type`) | `poca` | `ppo` |
| Terminalna nagrada ±1 grupnim kanalom | DA | NE (nije podržano) |
| `individual_terminal_reward` | 0,0 | 1,0 |
| Sastav timova | 2 lovca : 2 bjegunca | 2 lovca : 2 bjegunca |
| γ, oblikovanje, prepreke, opažanja, mreža, samoigra | JEDNAKI | JEDNAKI |
| Broj sjemena | 3 (1, 2, 3) | 3 (1, 2, 3) |

Razlika u kanalu isporuke nije proizvoljna, nego nužna. Algoritam PPO ne može koristiti grupne
nagrade — mehanizmi `AddGroupReward` i `EndGroupEpisode` isključivi su za MA-POCA trener — pa bi
ruka PPO bez te izmjene trenirala isključivo na vremenskom pritisku i nikada ne bi primila signal
pobjede ili poraza. Uvedeni parametar `individual_terminal_reward` zrcali terminalnu nagradu kroz
individualni kanal svakog agenta i time uspostavlja standardnu polazišnu crtu „neovisni agenti koji
uče iz dijeljene nagrade". **Tu razliku treba imati na umu pri tumačenju rezultata**: svaka
uočena razlika među rukama može potjecati i od kanala isporuke, a ne isključivo od algoritma.

**Pre-registrirane predikcije.** U skladu s pravilom uvedenim u Fazi A, predikcije su zapisane prije
pokretanja ijednog treninga:

- **P1 — bazna mreža se aktivira.** Omjer `Baseline Loss / Value Loss` odstupit će od jedinice pri
  veličini grupe većoj od jedan i prijeći vrijednost 1,05. *Opovrgnuto ako* ostane unutar pojasa
  1,002–1,006 zabilježenog u postavi jedan na jedan, što bi značilo da je bazna mreža neaktivna i u
  timskim uvjetima, a RQ-D neodgovoriv u ovom okruženju.
- **P2 — učinak je asimetričan u korist bjegunaca.** Budući da se deaktiviraju isključivo bjegunci,
  posmrtna dodjela zasluga primjenjuje se na grupu bjegunaca, pa će prednost algoritma MA-POCA biti
  veća u veličinama vezanima uz bjegunce nego u onima vezanima uz lovce. *Opovrgnuto ako* se ruke
  razlikuju pretežno na strani lovca ili podjednako na obje strane.
- **P3 — MA-POCA nadmašuje PPO u timskom ishodu.** Tim bjegunaca treniran algoritmom MA-POCA
  postići će veći udio preživljavanja od tima treniranog algoritmom PPO. *Opovrgnuto ako* su ruke
  nerazlučive unutar raspona među sjemenima — što je legitiman i objavljiv ishod koji bi
  jednakovrijednost utvrđenu u potpoglavlju 6.2 proširio s postave jedan na jedan na timove.
- **P4 — veličina učinka raste s brojem bjegunaca**, jer svaka deaktivacija otvara još jedan prozor
  posmrtne dodjele zasluga po epizodi. Provjerljivo samo ako proračun dopusti više od jednog sastava
  timova; zapisano unaprijed kako se ne bi moglo naknadno prisvojiti.

**Zaštita od poznatog čimbenika zabune.** ELO ocjena relativna je unutar pojedinog pokretanja i nije
kalibrirana između pokretanja, pa se usporedba između ruku provodi stopom hvatanja, udjelom
preživjelih bjegunaca i duljinom epizode, a ne ELO ocjenom.

---

## § 4 — Dopuna potpoglavlja 6.1 (Dokaz putem gubitka temeljne vrijednosti)

> **Umetnuti** na kraj postojećeg potpoglavlja 6.1, iza rečenice „Konačni dokaz jest simetričan:
> prisutnost BaselineLoss metrike potvrđuje MA-POCA algoritam, dok odsutnost iste metrike u PPO
> potvrđuje čisti PPO."

Navedeni dokaz potvrđuje da je korišten MA-POCA **trener**, ali — kako je utvrđeno naknadnom
analizom — ne potvrđuje da taj trener radi išta što PPO trener ne bi radio. Vrijedi obratiti pozornost
na same brojeve u Tablici 6.1: gubitak vrijednosne glave iznosi 0,0202, a gubitak kontrafaktične
bazne mreže 0,0202 odnosno 0,0206. Te dvije veličine nisu samo sličnog reda veličine — one su
praktički identične, i to nije slučajnost.

Uvidom u izvorni kod trenera (`POCAOptimizer`) razvidno je zašto. Kontrafaktična bazna procjena
računa se pozivom `baseline(obs_without_actions, obs_with_actions)`, gdje prvi argument sadrži
opažanja promatranog agenta, a drugi opažanja i akcije njegovih **suigrača**. U postavi jedan na
jedan agent nema suigrača, pa je popis suigrača prazan i taj se poziv svodi na `critic_pass(obs)` —
dakle na potpuno isti izračun koji provodi obična vrijednosna glava. Prednost tada iznosi
`povrat − V`, što je točno izraz koji koristi PPO. Jedina razlika koja preostaje jest da funkcija
gubitka kritičara u algoritmu MA-POCA nosi dodatni član `0,5 · baseline_loss`, čime se učinkoviti
koeficijent uz gubitak vrijednosti mijenja s 0,5 na 0,75.

Drugim riječima: **u postavi jedan na jedan algoritam MA-POCA matematički je jednak algoritmu PPO uz
razliku u jednom hiperparametru.** To nije razlika u algoritmu, nego u ugađanju.

Tvrdnja je mjerljiva i provjerena je na svim pokretanjima u postavi jedan na jedan: omjer
`Baseline Loss / Value Loss` kreće se u pojasu 1,002–1,006, a njegov je najmanji zabilježeni iznos
**točno 1,000**. Time se retroaktivno objašnjava nalaz iz potpoglavlja 6.2, gdje se pri rijetkoj
nagradi algoritmi MA-POCA i PPO nisu razlikovali: to nije bila empirijska podudarnost, nego identitet.

Upravo je ta spoznaja motivirala Fazu C, opisanu u potpoglavlju 5.8: da bi se algoritmi uopće mogli
usporediti, veličina grupe mora prijeći jedan.

---

## § 5 — Novo potpoglavlje 6.7: Rezultati Faze C

> **Umetnuti** kao novo potpoglavlje razine 2, iza potpoglavlja 6.6.

### 6.7 Rezultati Faze C — MA-POCA nasuprot PPO u postavi dva na dva

Provedeno je šest pokretanja po 5 · 10⁶ koraka: tri sjemena po ruci treniranja. Sva su pokretanja
uredno dovršena — dvadeset kontrolnih točaka, izvezeni modeli u formatu ONNX i **nijedna
nekonačna vrijednost** ni u jednoj zabilježenoj veličini. Trajanje po pokretanju iznosilo je između
4,6 i 6,8 sati na grafičkom procesoru. U svakom je zapisniku Unity procesa potvrđena postava
(`[TeamManager] num_chasers=2, num_runners=2`).

[TABLICA — Tablica 6.9 Rezultati Faze C — šest pokretanja pri postavi dva na dva]

| Pokretanje | Stopa hvatanja | Udio preživjelih bjegunaca | Duljina epizode | Vrijeme do hvatanja | ELO razlika | Ishod |
|---|---|---|---|---|---|---|
| MA-POCA, sjeme 1 | 0,000 | 0,967 | 399,0 | 1061 | 19 | **urušavanje** |
| MA-POCA, sjeme 2 | 0,979 | 0,011 | 70,3 | 163 | 986 | naučeno |
| MA-POCA, sjeme 3 | 0,998 | 0,001 | 53,8 | 136 | 1115 | naučeno |
| PPO, sjeme 1 | 0,982 | 0,009 | 91,2 | 220 | 1015 | naučeno |
| PPO, sjeme 2 | 0,000 | 0,974 | 399,0 | 1120 | 39 | **urušavanje** |
| PPO, sjeme 3 | 0,996 | 0,002 | 67,2 | 168 | 1099 | naučeno |

Ishod je u obje ruke jednak: **dva od tri sjemena nauče progon, a jedno se uruši.**

[SLIKA — Slika 6.32]
Datoteka: `docs/figures/phasec/tb_catch_6runs.png`
TensorBoard oznaka: `Environment/Catch`, svih šest pokretanja na istoj osi; ruka MA-POCA punom
linijom, ruka PPO isprekidanom.
Predloženi potpis: *Slika 6.32 Stopa hvatanja (Environment/Catch) za svih šest pokretanja Faze C,
5 · 10⁶ koraka*

[SLIKA — Slika 6.33]
Datoteka: `docs/figures/phasec/tb_runners_survived.png`
TensorBoard oznaka: `Environment/RunnersSurvived`, svih šest pokretanja.
Predloženi potpis: *Slika 6.33 Udio preživjelih bjegunaca (Environment/RunnersSurvived), Faza C,
5 · 10⁶ koraka*

[SLIKA — Slika 6.34]
Datoteka: `docs/figures/phasec/tb_elo_6runs.png`
TensorBoard oznaka: `Self-play/ELO` za obje uloge, svih šest pokretanja.
Predloženi potpis: *Slika 6.34 ELO ocjena u samoigri (Self-play/ELO), Faza C; kod dvaju urušenih
pokretanja obje ocjene ostaju na polazišnoj vrijednosti 1200*

#### 6.7.1 P1 — bazna mreža se aktivira (predikcija potvrđena)

Odstupanje kontrafaktične bazne mreže od vrijednosne funkcije mjereno je srednjom udaljenošću omjera
`Baseline Loss / Value Loss` od jedinice, računatom kroz svih sto zapisa pojedinog pokretanja:

[TABLICA — Tablica 6.10 Odstupanje bazne mreže od vrijednosne funkcije pri veličini grupe 1 i 2]

| | sjeme 1 | sjeme 2 | sjeme 3 |
|---|---|---|---|
| Lovac, postava 1v1 (Faza B) | 0,016 | 0,027 | 0,018 |
| **Lovac, postava 2v2** | **0,153** | **0,206** | **0,227** |
| Bjegunac, postava 1v1 (Faza B) | 0,026 | 0,017 | 0,017 |
| **Bjegunac, postava 2v2** | **0,106** | **0,124** | **0,087** |

Dvanaest mjerenja, **bez ijednog preklapanja**: svaka vrijednost izmjerena pri postavi dva na dva
veća je od svake vrijednosti izmjerene pri postavi jedan na jedan, uz razdvajanje od pet do devet
puta. Pritom je najmanji zabilježeni omjer u postavi jedan na jedan točno 1,000 u svih šest
promatranih ponašanja, dok u postavi dva na dva omjer pada i **ispod** jedinice (najmanje 0,835) —
što je moguće jedino ako dvije glave doista računaju različite veličine.

Najvažnije je da mjerenje **ne ovisi o ishodu treniranja**. Pokretanje s najvećim odstupanjem
(0,227) ujedno je i najuspješnije pokretanje cijele faze (stopa hvatanja 0,998), dok se pokretanje
sa sjemenom 1 posve urušilo, a i dalje bilježi odstupanje od 0,153. Odstupanje, dakle, prati veličinu
grupe, a ne uspješnost učenja — točno kako predviđa izvod iz potpoglavlja 6.1.

[SLIKA — Slika 6.35]
Datoteka: `docs/figures/phasec/baseline_value_ratio.png`
Sadržaj: omjer `Losses/Baseline Loss ÷ Losses/Value Loss` u ovisnosti o koraku treniranja; tri
pokretanja postave jedan na jedan (ravna linija na 1,00) nasuprot trima pokretanjima postave dva na
dva (oscilacije između 0,84 i 1,65).
Predloženi potpis: *Slika 6.35 Omjer gubitka bazne mreže i gubitka vrijednosne funkcije pri veličini
grupe 1 i 2*

Uz ovaj nalaz nužno je navesti i ogradu: pokretanja u postavi jedan na jedan, koja služe kao
usporedba, provedena su prije programske preinake nužne za višeagentnu fazu, pa se usporedba
provodi između dviju inačica koda. Planirano kontrolno pokretanje u postavi jedan na jedan na
istom kodu nije provedeno zbog ograničenja raspoloživog vremena. Izvod iz potpoglavlja 6.1 ne ovisi
o inačici koda, a najmanji izmjereni omjer od točno 1,000 dodatno ga potkrepljuje, no empirijski bi
dio tvrdnje bio jači uz to kontrolno pokretanje.

#### 6.7.2 P2 i P3 — predikcije opovrgnute: nema prednosti u timskom ishodu

Predikcija **P2** očekivala je da će se prednost algoritma MA-POCA očitovati pretežno u veličinama
vezanima uz bjegunce, jer se deaktiviraju isključivo bjegunci. **Opovrgnuta je.** Udio preživjelih
bjegunaca praktički je jednak u obje ruke (0,011 i 0,001 kod uspješnih pokretanja MA-POCA naspram
0,009 i 0,002 kod uspješnih pokretanja PPO), dok se razlika koja postoji pojavljuje na strani
**lovca** — dakle upravo u uvjetu koji je unaprijed naveden kao opovrgavajući.

Predikcija **P3** očekivala je veći udio preživljavanja bjegunaca u ruci MA-POCA. **Opovrgnuta je**,
i to na onoj grani koja je pre-registracijom izrijekom predviđena kao legitiman ishod: ruke su
nerazlučive unutar raspona među sjemenima. Konačna stopa hvatanja (0,979 i 0,998 naspram 0,982 i
0,996) te ELO razlika (986 i 1115 naspram 1015 i 1099) praktički su izjednačene.

Predikcija **P4** ostaje **neprovjerena**, jer je proračun dopustio samo jedan sastav timova.

#### 6.7.3 Nepredviđeni nalaz: razmjena brzine učenja za kvalitetu igre

Ako se promatraju isključivo četiri pokretanja koja su naučila progon, pojavljuju se dva jasna i
međusobno suprotna razdvajanja, pri čemu se rasponi ne preklapaju:

[TABLICA — Tablica 6.11 Usporedba uspješnih pokretanja po rukama treniranja, Faza C]

| Veličina | MA-POCA (sjemena 2 i 3) | PPO (sjemena 1 i 3) |
|---|---|---|
| Koraci do stope hvatanja iznad 0,10 | 2,5 · 10⁶ i 1,4 · 10⁶ | **0,55 · 10⁶ i 0,20 · 10⁶** |
| Konačna stopa hvatanja | 0,979 i 0,998 | 0,982 i 0,996 |
| ELO razlika | 986 i 1115 | 1015 i 1099 |
| Vrijeme do hvatanja (manje je bolje) | **136 i 163** | 168 i 220 |

Algoritam PPO počinje hvatati tri do sedam puta ranije, dok algoritam MA-POCA, jednom kad postane
uspješan, hvata približno dvadeset posto brže. Konačna je uspješnost izjednačena.

Objašnjenje je uvjerljivo, ali ga treba iznijeti oprezno. Terminalna nagrada isporučena individualno
gušći je i jednoznačniji signal u ranoj fazi učenja, dok se prednost centralizirane bazne mreže može
očitovati tek kada koordinirano ponašanje uopće postoji. Ipak, s po dva uspješna pokretanja po ruci
riječ je o **naznaci, a ne o utvrđenoj činjenici**, a nalaz je dodatno opterećen razlikom u kanalu
isporuke nagrade opisanom u potpoglavlju 5.8. Duljina epizode pokazuje isti smjer, no ondje se
rasponi dodiruju.

#### 6.7.4 Dvostabilnost: trećina pokretanja nikada se ne pokrene

Dva od šest pokretanja (33 %) završila su sa stopom hvatanja **točno 0,000** i sa svim epizodama na
gornjoj granici od 399 koraka odlučivanja. Potpis je urušavanja u oba slučaja isti: entropija
politike lovca **raste** (s 1,41 na 1,60 odnosno 1,63) dok entropija bjegunca pada, procjena
vrijednosti lovca ostaje prikovana oko −1,3, a obje ELO ocjene ostaju na polazišnoj vrijednosti
1200, što znači da svaka epizoda završava neriješeno.

Treće je sjeme dalo odlučujući dokaz o uzroku. **Sjeme 1 urušilo se u ruci MA-POCA, ali je u ruci
PPO naučilo; sjeme 2 naučilo je u ruci MA-POCA, ali se u ruci PPO urušilo.** Budući da vrijednost
`--seed` upravlja i generatorom nasumičnih brojeva za raspored prepreka i za stvaranje agenata, i to
jednako u obje ruke, rasporedi koje je proizvelo sjeme 1 dokazano su bili takvi da se u njima može
hvatati — jer je u njima ruka PPO hvatala. **Urušavanje stoga nije posljedica nepovoljne geometrije
arene, nego dinamike samog treniranja**: samoigra se zaključa u neriješen ishod prije nego što lovac
uopće otkrije prvo hvatanje. Time otpada objašnjenje o „nesretnom rasporedu prepreka".

Valja pridodati i da u postavi dva na dva stopa hvatanja poprima vrijednost 1 tek ako su uhvaćena
**oba** bjegunca, pa je događaj koji pokreće učenje rjeđi nego u postavi jedan na jedan, u kojoj se
u ovom radu nije urušilo nijedno pokretanje.

[SLIKA — Slika 6.36]
Datoteka: `docs/figures/phasec/tb_entropy_collapse.png`
TensorBoard oznaka: `Policy/Entropy` za dva urušena pokretanja i dva uspješna.
Predloženi potpis: *Slika 6.36 Entropija politike (Policy/Entropy) kod urušenih i uspješnih
pokretanja Faze C; rastuća entropija lovca potpis je urušavanja*

---

## § 6 — Novo potpoglavlje 6.8: Rasprava rezultata — Faza C

> **Umetnuti** iza potpoglavlja 6.7.

### 6.8 Rasprava rezultata — Faza C

Istraživačko pitanje RQ-D glasilo je ostvaruje li algoritam MA-POCA mjerljivu prednost pred
algoritmom PPO kada veličina grupe prijeđe jedan. Odgovor koji proizlazi iz provedenih pokusa jest
razmjerno neobičan: **razlikovni se mehanizam algoritma dokazano uključuje, ali se to ne pretače u
bolji ishod igre.**

Prvi je dio tvrdnje utvrđen. U postavi jedan na jedan bazna je mreža neaktivna, i to iz načelnih
razloga izvedenih iz izvornog koda, a ne zbog nedostatka mjerne osjetljivosti; u postavi dva na dva
ona je aktivna, s razdvajanjem koje u dvanaest mjerenja nema nijednog preklapanja i koje ne ovisi o
tome je li pokretanje uspjelo. Time je popunjena praznina na koju je upozoreno u potpoglavlju 6.2:
tamošnja jednakovrijednost algoritama MA-POCA i PPO nije bila mjerni artefakt niti posljedica
premalog broja koraka, nego nužna posljedica veličine grupe.

Drugi je dio tvrdnje negativan i tako ga treba i iznijeti. Konačna stopa hvatanja, udio preživjelih
bjegunaca i ELO razlika izjednačeni su među rukama, a obje pre-registrirane predikcije o boljem
timskom ishodu opovrgnute su. Time se jednakovrijednost utvrđena u postavi jedan na jedan proširuje
i na postavu dva na dva — što je, doduše, ishod koji je pre-registracijom unaprijed prepoznat kao
legitiman i objavljiv, ali ostaje ishod suprotan početnom očekivanju.

Treći je nalaz nepredviđen i po mišljenju autora najzanimljiviji praktični rezultat ove faze:
razmjena između brzine učenja i kvalitete konačne igre. Algoritam PPO postiže prve uspjehe znatno
ranije, dok algoritam MA-POCA, jednom kada postane uspješan, igra nešto učinkovitije. Za praktičara
to znači da izbor između tih dvaju pristupa u zadatku ovog tipa nije pitanje „koji je bolji", nego
pitanje raspoloživog proračuna koraka treniranja.

Četvrti nalaz nadilazi usporedbu algoritama. Trećina pokretanja nikada nije naučila progon, i to
neovisno o algoritmu, a pokazano je da uzrok nije geometrija arene nego dinamika samoigre. Taj je
rezultat izravan protuteg motivacijskom primjeru iz uvoda: javno dostupni prikazi emergentnog
ponašanja redovito prikazuju jedno uspješno pokretanje, dok se ovdje, uz nepromijenjene uvjete,
svako treće pokretanje uopće ne pokreće s mrtve točke. **Emergentno ponašanje u kompetitivnoj
samoigri stvarno je, ali nije pouzdano**, i tu razliku valja imati na umu pri svakom prijenosu ovih
metoda u praksu.

Ograničenja su ove faze jasna i navode se bez ublažavanja. Ispitan je samo jedan sastav timova (dva
na dva). Po ruci su provedena tri sjemena, a ishod je dvostabilan, pa usporedba ima malu razlučivost
i iz nje se ne smije izvoditi tvrdnja jača od „nerazlučivo". Usporedna pokretanja u postavi jedan na
jedan potječu iz ranije inačice koda. Naposljetku, ruke se nužno razlikuju i u kanalu isporuke
terminalne nagrade, jer algoritam PPO grupne nagrade uopće ne podržava.

Mjesto na kojemu bi se očekivani učinak trebao tražiti sljedeće jasno je određeno: veći timovi
bjegunaca, dakle sastavi tri na tri i asimetrični dva na tri, u kojima svaka dodatna deaktivacija
otvara još jedan prozor posmrtne dodjele zasluga po epizodi. To je upravo predikcija P4, koja je u
ovom radu ostala neprovjerena. Konfiguracijske datoteke i izvršna inačica okruženja to već
podržavaju bez ijedne izmjene koda — nedostaje isključivo računalno vrijeme.

---

## § 7 — ZAKLJUČAK

> **Umetnuti** pod postojeći naslov „ZAKLJUČAK", koji je trenutačno prazan.

Cilj ovog rada bio je ispitati može li algoritam MA-POCA u kombinaciji sa samoigrom razviti
emergentne strategije progona i bijega u asimetričnoj kompetitivnoj igri, bez eksplicitnog
programiranja tih ponašanja. Odgovor je potvrdan, ali je put do njega otkrio niz nalaza koji su
zanimljiviji od samog odgovora.

U okviru rada implementirano je okruženje igre Lovice u alatu Unity uz okvir ML-Agents, s dvama
zasebnim ponašanjima i dvjema grupama agenata, te je provedeno **dvadeset devet pokretanja treniranja
po 5 · 10⁶ koraka**, ukupno oko 145 · 10⁶ koraka simulacije i približno 132 sata računalnog vremena,
raspoređenih u četiri unaprijed osmišljena eksperimenta. Od faze pretraživanja diskontnog faktora
nadalje primjenjivano je pravilo zapisivanja predikcija prije pokretanja pokusa, čime je onemogućeno
naknadno prilagođavanje hipoteze rezultatu.

**Emergentno ponašanje ne treba oblikovanu nagradu.** Sama terminalna nagrada, isporučena tek na
kraju epizode, dovoljna je da se razvije odlučan progon: u rijetkoj ruci lovac pobjeđuje u svim
sjemenima, uz ELO razliku iznad tisuću bodova. To je izravan odgovor na središnje istraživačko
pitanje rada.

**Oblikovanje nagrade zasnovano na potencijalu pokazalo se zamkom.** Iako teorem o invarijantnosti
politike jamči da takvo oblikovanje ne mijenja optimalnu politiku, ono mijenja **putanju učenja**:
lovac se urušio u parazitsko skupljanje nagrade, uz grupnu nagradu od približno −1 i stopu hvatanja
oko 0,01, dok je individualna nagrada ostajala visoka. Utvrđena su dva odvojena uzroka — kanal
isporuke terminalne nagrade i osjetljivost samog algoritma — a mehanizam je kvantitativno potvrđen
kroz ovisnost o članu (1 − γ). Praktična je pouka izravna: nagrada koja mjeri ono što se lako mjeri,
a ne ono što se doista želi, bit će iskorištena upravo onako kako je zapisana.

**Kratkoročna validacija može dati suprotan zaključak od ispravnog.** Na 400 000 koraka oblikovana
je ruka izgledala bolje; na 5 · 10⁶ koraka rang se obrnuo. Zaključci o odabiru funkcije nagrade
doneseni na malom proračunu koraka nisu pouzdani.

**Vrijednost γ = 0,99 dobila je empirijsko opravdanje.** Ispod nje pojavljuje se porez na
kratkovidnost, iznad nje rizik nestabilnosti, a upravo pri toj vrijednosti učenje je najbrže uz
vrhunske konačne rezultate.

**Nasumičan raspored prepreka ne mijenja ravnotežu.** Pre-registrirana predikcija da će nasumični
rasporedi učiti sporije i donositi slabije rezultate opovrgnuta je u oba svoja dijela, čime je
otklonjena i sumnja da je lovac zapamtio geometriju arene umjesto da je naučio reaktivnu navigaciju.

**Naposljetku, sam je naslovni algoritam podvrgnut provjeri.** Utvrđeno je — izvodom iz izvornog
koda trenera i potvrđeno mjerenjem — da u postavi jedan na jedan kontrafaktična bazna mreža nema o
čemu zaključivati te da je algoritam MA-POCA u toj postavi jednak algoritmu PPO uz razliku u jednom
hiperparametru. Tek uvođenjem timova u postavi dva na dva bazna se mreža aktivira, uz razdvajanje
bez preklapanja u dvanaest mjerenja. Međutim, ta se aktivacija pri toj veličini tima **ne pretače u
bolji ishod igre**: konačna stopa hvatanja, udio preživjelih bjegunaca i ELO razlika izjednačeni su
među algoritmima, uz nepredviđenu razmjenu u kojoj algoritam PPO uči znatno ranije, a algoritam
MA-POCA igra nešto učinkovitije kada nauči.

Uz to je zabilježen i nalaz koji nadilazi usporedbu algoritama: u postavi dva na dva **trećina se
pokretanja nikada nije pokrenula s mrtve točke**, neovisno o algoritmu, a pokazano je da uzrok nije
geometrija arene nego zaključavanje samoigre u neriješen ishod. Emergentno je ponašanje, dakle,
stvarno, ali nije pouzdano — što je važna ograda pri svakom prijenosu ovih metoda u primjenu.

Ograničenja rada iznose se bez ublažavanja. Sva su mjerenja provedena u jednom okruženju i na jednom
zadatku. Broj sjemena po konfiguraciji kreće se između jednog i tri, što je dovoljno za opis, ali ne
i za jaku statističku tvrdnju, osobito ondje gdje je ishod dvostabilan. ELO ocjena relativna je
unutar pojedinog pokretanja i nije kalibrirana između pokretanja. Usporedna pokretanja u postavi
jedan na jedan potječu iz ranije inačice koda, pa kontrolno pokretanje na istom kodu ostaje
neprovedeno. Naposljetku, u višeagentnoj se fazi ruke nužno razlikuju i u kanalu isporuke terminalne
nagrade, jer algoritam PPO grupne nagrade uopće ne podržava.

Za budući se rad nameću četiri smjera. Prvi je kontrolno pokretanje u postavi jedan na jedan na
istom kodu, čime bi se zatvorila jedina preostala ograda uz glavni nalaz. Drugi su veći i asimetrični
timovi (tri na tri, dva na tri), gdje svaka dodatna deaktivacija otvara još jedan prozor posmrtne
dodjele zasluga — dakle upravo uvjet u kojem bi se očekivana prednost algoritma MA-POCA trebala
pojaviti, a koji u ovom radu nije ispitan. Treći je procjena učestalosti urušavanja na većem broju
sjemena, uz ispitivanje može li se ono izbjeći kurikulumom ili početnom pomoći pri prvom hvatanju.
Četvrti je istraživanje praga oblikovanja: zamka je utvrđena pri koeficijentu 0,5, a otvoreno je
pitanje postoji li blaži iznos koji pomaže, a ne šteti.

Naposljetku valja istaknuti i praktično ograničenje pod kojim je rad nastao. Sva su pokretanja
provedena na jednom osobnom računalu, uz trajanje između otprilike četiri i sedam sati po pokretanju,
što je izravno odredilo broj sjemena i broj ispitanih konfiguracija. **Dulje treniranje i veći broj
sjemena na snažnijoj računalnoj opremi — primjerice na poslužitelju s više grafičkih procesora ili u
oblaku — predstavljali bi vrijedan smjer za buduće pokuse**, jer bi omogućili ispitivanje većih
timova, pouzdaniju procjenu učestalosti urušavanja i statistički jaču usporedbu algoritama, koja je
u ovom radu iz proračunskih razloga mogla biti samo opisna.

Ukupno, rad je od početnog pitanja „radi li to uopće" napredovao do reproducibilne i unaprijed
registrirane karakterizacije uvjeta u kojima to radi, zašto radi i kada ne radi — što se, po
mišljenju autora, pokazalo vrjednijim doprinosom od pukog ponavljanja demonstracije koja ga je
potaknula.

---

## § 8 — SAŽETAK / ABSTRACT I KLJUČNE RIJEČI / KEYWORDS

> **Umetnuti** pod postojeći naslov, u pripadajuća prazna polja.

**Naslov:** Analiza kompetitivne interakcije u video igrama pomoću multi-agentskog strojnog učenja

**Sažetak**

U radu se ispituje mogu li se emergentne strategije progona i bijega razviti isključivo iz signala
nagrade, bez eksplicitnog programiranja ponašanja. Implementirana je asimetrična kompetitivna igra
Lovice u alatu Unity uz okvir ML-Agents, s lovcem i bjeguncem kao dvama odvojenim ponašanjima
treniranima algoritmom MA-POCA u režimu samoigre. Provedeno je dvadeset devet pokretanja treniranja
po pet milijuna koraka, raspoređenih u četiri unaprijed osmišljena eksperimenta, uz pravilo
zapisivanja predikcija prije pokretanja pokusa. Utvrđeno je da rijetka terminalna nagrada dostaje za
razvoj odlučnog progona, dok oblikovanje nagrade zasnovano na potencijalu vodi u zamku parazitskog
skupljanja nagrade unatoč teoremu o invarijantnosti politike; rang dviju ruku treniranja pritom se
obrće između kratkog i punog trajanja pokusa. Pretraživanjem diskontnog faktora empirijski je
opravdana uobičajena vrijednost 0,99, a nasumičan raspored prepreka pokazao se bez mjerljivog
utjecaja na ravnotežu igre. Naposljetku je izvodom iz izvornog koda trenera i mjerenjem pokazano da
je algoritam MA-POCA u postavi jedan na jedan jednak algoritmu PPO uz razliku u jednom
hiperparametru te da se njegova kontrafaktična bazna mreža aktivira tek uvođenjem timova, pri čemu
ta aktivacija u postavi dva na dva ne donosi bolji ishod igre. Zabilježeno je i da se trećina
višeagentnih pokretanja nikada ne pokrene s mrtve točke, neovisno o algoritmu.

**Ključne riječi:** pojačano strojno učenje, višeagentno učenje, MA-POCA, PPO, samoigra, emergentno
ponašanje, oblikovanje nagrade, Unity ML-Agents, igra Lovice, dodjela zasluga

**Title:** Analysis of Competitive Interaction in Video Games Using Multi-Agent Machine Learning

**Abstract**

This thesis investigates whether emergent pursuit and evasion strategies can arise purely from a
reward signal, without explicitly programming the behaviour. An asymmetric competitive game of tag
was implemented in Unity using the ML-Agents framework, with the chaser and the runner as two
separate behaviours trained by the MA-POCA algorithm under self-play. Twenty-nine training runs of
five million steps each were carried out across four designed experiments, following a rule that
predictions be recorded before each experiment was launched. A sparse terminal reward proved
sufficient for decisive pursuit to emerge, whereas potential-based reward shaping led the chaser
into a reward-farming trap despite the policy-invariance theorem; the ranking of the two training
arms reversed between the short validation budget and the full run. A discount-factor sweep provided
empirical justification for the conventional value of 0.99, and randomising the obstacle layout had
no measurable effect on the competitive balance. Finally, a derivation from the trainer source code,
confirmed by measurement, showed that at a group size of one MA-POCA is equivalent to PPO up to a
single hyperparameter, and that its counterfactual baseline becomes active only once teams are
introduced — an activation that, at two versus two, does not translate into a better game outcome.
It was further observed that one third of the multi-agent runs never left the ground, independently
of the algorithm.

**Keywords:** reinforcement learning, multi-agent reinforcement learning, MA-POCA, PPO, self-play,
emergent behaviour, reward shaping, Unity ML-Agents, tag game, credit assignment

---

## § 9 — Dopune popisa oznaka i kratica

> **Dodati** postojećem popisu, uz zadržavanje abecednog reda.

| Kratica | Objašnjenje |
|---|---|
| BufferSensor | međuspremničko osjetilo u okviru ML-Agents; prima promjenjiv broj entiteta i permutacijski je invarijantno |
| ONNX | *Open Neural Network Exchange* — format u kojem se izvoze istrenirani modeli *(već postoji u popisu, provjeriti)* |
| PBS | oblikovanje nagrade zasnovano na potencijalu *(već postoji u popisu, provjeriti)* |
| RQ-D | četvrto istraživačko pitanje: ostvaruje li MA-POCA prednost pred PPO pri veličini grupe većoj od jedan |
| StatsRecorder | sučelje okvira ML-Agents za bilježenje vlastitih mjernih veličina na razini okoline |
| YAML | *YAML Ain't Markup Language* — format konfiguracijskih datoteka trenera |

---

## § 10 — Kontrolni popis novih stavaka kazala

Word generira kazala automatski nakon umetanja potpisa; ovaj popis služi samo za provjeru da nijedan
potpis nije izostavljen.

**Kazalo slika — nove stavke:**

- Slika 6.32 Stopa hvatanja (Environment/Catch) za svih šest pokretanja Faze C, 5 · 10⁶ koraka
- Slika 6.33 Udio preživjelih bjegunaca (Environment/RunnersSurvived), Faza C, 5 · 10⁶ koraka
- Slika 6.34 ELO ocjena u samoigri (Self-play/ELO), Faza C
- Slika 6.35 Omjer gubitka bazne mreže i gubitka vrijednosne funkcije pri veličini grupe 1 i 2
- Slika 6.36 Entropija politike (Policy/Entropy) kod urušenih i uspješnih pokretanja Faze C

**Kazalo tablica — nove stavke:**

- Tablica 5.3 Dizajn ruku treniranja u Fazi C
- Tablica 6.9 Rezultati Faze C — šest pokretanja pri postavi dva na dva
- Tablica 6.10 Odstupanje bazne mreže od vrijednosne funkcije pri veličini grupe 1 i 2
- Tablica 6.11 Usporedba uspješnih pokretanja po rukama treniranja, Faza C

---

## § 11 — Ostali prilozi i dokumentacija

> **Umetnuti** pod postojeći naslov, koji je trenutačno prazan.

Cjelokupan izvorni kod okruženja, konfiguracijske datoteke trenera, skripte za pokretanje pokusa i
skripte za obradu rezultata pohranjeni su u sustavu za upravljanje inačicama uz rad. Struktura je
sljedeća:

**Izvorni kod okruženja (Unity, C#)**

- `Assets/Scripts/TagAgent.cs` — logika agenta, prostor opažanja i prostor akcija za obje uloge
- `Assets/Scripts/TagArenaManager.cs` — upravljanje epizodama, isporuka nagrada, ponovno postavljanje
  arene i bilježenje vlastitih mjernih veličina
- `Assets/Scripts/TeamManager.cs` — aktivacija zadanog broja lovaca i bjegunaca prema parametrima
  okoline
- `Assets/Scripts/ObstacleManager.cs` — postavljanje prepreka u sceni, fiksno i nasumično
- `Assets/Scripts/Reward/TagReward.cs` — matematika nagrade, izdvojena u zasebnu jedinicu radi
  jediničnog testiranja
- `Assets/Scripts/Reward/ObstaclePlacement.cs` — čisti izračun rasporeda prepreka metodom odbacivanja
- `Assets/Scripts/Reward/SpawnPlacement.cs` — postavljanje N agenata uz zadani najmanji razmak
- `Assets/Tests/EditMode/` — trideset tri jedinična testa nad matematikom nagrade, raspoređivanjem
  prepreka i postavljanjem agenata

**Konfiguracijske datoteke trenera** (`config/poca/`) — po jedna datoteka za svaku ispitanu
konfiguraciju, uključujući rijetku i oblikovanu ruku, ruke algoritma PPO, pet vrijednosti diskontnog
faktora te postave dva na dva i tri na tri.

**Skripte za pokretanje pokusa** (`experiments/`) — paketne datoteke koje redom pokreću sve
konfiguracije pojedine faze, s provjerama ispravnosti okruženja prije pokretanja.

**Skripte za obradu rezultata** (`experiments/analysis/`) — samostalna skripta za izlučivanje
mjernih veličina iz TensorBoard zapisa bez vanjskih ovisnosti te skripta za izradu grafova.

**Istrenirani modeli** — konačne mreže svih pokretanja u formatu ONNX, spremne za pokretanje u
načinu izvođenja (engl. *inference*) unutar Unity uređivača.

**Dokumentacija istraživanja** — dnevnik rada po sesijama te dokument s teorijskim izvodima i
empirijskim nalazima, u kojem su zapisane i sve pre-registrirane predikcije s datumima nastanka,
prije pokretanja pripadajućih pokusa.

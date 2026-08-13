# Nacrt — potpoglavlje o konfiguracijskoj datoteci

> Tekst za lijepljenje u rad. Sve brojčane vrijednosti odgovaraju datoteci
> `TagMApoca_shaped_5M.yaml` (oblikovana ruka, 5 milijuna koraka), provjerenoj na disku
> 9. 8. 2026. Prijedlog mjesta: poglavlje 5, neposredno prije opisa pojedinih pokusa.

---

## Uloga konfiguracijske datoteke

Postupak treniranja u Unity ML-Agentsu razdvojen je na dva dijela: okolina je definirana u
Unityju i prevedena u izvršnu datoteku, dok su svi parametri učenja zadani izvana, u
konfiguracijskoj datoteci koja se predaje treneru pri pokretanju. To razdvajanje nije stvar
udobnosti nego preduvjet ponovljivosti. Kada bi se vrijednosti poput koeficijenta oblikovanja
nagrade ili faktora umanjenja postavljale ručno u Unity Editoru, svaki bi pokus ovisio o stanju
scene u trenutku pokretanja, a to se stanje ne bi moglo pouzdano rekonstruirati niti dokumentirati.
Ovako je cjelokupna specifikacija pokusa sadržana u jednoj tekstualnoj datoteci koja se može
verzionirati, usporediti s drugom datotekom i priložiti radu kao dokaz o tome što je točno
pokrenuto.

Ta je odluka izravno oblikovala eksperimentalni dizajn ovog rada. Rijetka i oblikovana ruka
razlikuju se isključivo po jednoj vrijednosti u konfiguraciji — `distance_shaping_coef`, koja
iznosi 0,0 odnosno 0,5 — dok su okolina, izvršna datoteka i sav ostali kod potpuno isti. Time je
uklonjena čitava klasa mogućih zabuna u kojima bi razlika između ruku proizlazila iz nečega drugog
osim iz namjeravane manipulacije.

## Zašto YAML

Format YAML odabran je zato što ga ML-Agents očekuje kao svoj izvorni format, ali i zato što
odgovara prirodi podataka koji se opisuju. Konfiguracija je hijerarhijska: svako ponašanje ima svoj
blok, unutar njega postoje odvojene skupine hiperparametara, arhitekture mreže, signala nagrade i
samoigre. YAML tu hijerarhiju izražava uvlakama, bez zagrada i navodnika, pa je datoteka čitljiva i
osobi koja nikada nije radila s ML-Agentsom. Za razliku od formata JSON, YAML dopušta komentare, što
je iskorišteno u zaglavlju svake konfiguracije ovog rada: prva četiri retka datoteke
`TagMApoca_shaped_5M.yaml` navode namjenu datoteke, njezinu razliku u odnosu na rijetku ruku,
podrijetlo hiperparametara i uputu da se pokreće s trima sjemenima. Time je namjera pokusa
zabilježena na istom mjestu gdje i njegovi parametri.

## Smještaj datoteka i način pozivanja

Konfiguracijske datoteke ne nalaze se u repozitoriju Unity projekta, nego u kloniranom repozitoriju
alata ML-Agents, u mapi `config/poca/`. Razlog je praktičan: naredba `mlagents-learn` pokreće se iz
korijena tog repozitorija, pa se datoteke navode relativnim putem, jednako kao i primjeri koje alat
donosi (`SoccerTwos.yaml`, `StrikersVsGoalie.yaml`). Trener se pokreće naredbom oblika

```
mlagents-learn config/poca/TagMApoca_shaped_5M.yaml --run-id=POCA_shaped_s1 --seed 1
```

pri čemu `--run-id` određuje ime mape u koju se spremaju rezultati, a `--seed` sjeme generatora
slučajnih brojeva, čime se ista konfiguracija pokreće tri puta uz različitu inicijalizaciju.

Budući da bi takav smještaj otežao ponovljivost — repozitorij alata nije dio ovog rada — svaka je
konfiguracija dodatno arhivirana u repozitoriju projekta, u mapi `experiments/configs/`. Uz to,
ML-Agents pri svakom pokretanju zapisuje datoteku `results/<run-id>/configuration.yaml` s potpuno
razriješenom konfiguracijom, uključujući i vrijednosti koje nisu bile eksplicitno navedene. Ta je
datoteka mjerodavan zapis o tome što je trener stvarno koristio i upravo je iz nje provjerena
podudarnost postavki među uspoređivanim rukama.

## Struktura datoteke

Datoteka ima dva dijela. Prvi je blok `behaviors`, koji sadrži dva potpuno odvojena podbloka,
`Chaser` i `Runner`. Ta podjela izravno slijedi iz asimetrične postave opisane u potpoglavlju o
implementaciji: budući da progonitelj i bjegunac rješavaju različite zadatke, svaki od njih ima
vlastito ime ponašanja i vlastiti identifikator tima, pa mu pripada i vlastiti blok konfiguracije.
U ovom su radu oba bloka namjerno postavljena na identične vrijednosti kako razlika u ponašanju
dvaju agenata ne bi mogla proizaći iz razlike u postavkama učenja, nego isključivo iz njihovih
suprotstavljenih ciljeva. Drugi je dio blok `environment_parameters`, koji se ne odnosi na trener
nego se prosljeđuje samoj okolini.

## Hiperparametri optimizacije

Veličina serije `batch_size` iznosi 2048, a veličina spremnika iskustva `buffer_size` 40960, dakle
točno dvadeset serija po jednom ažuriranju. Trener prikuplja iskustvo dok se spremnik ne napuni, a
zatim nad njim provede `num_epoch` = 5 prolaza, čime se svako prikupljeno iskustvo iskoristi pet
puta prije nego što bude odbačeno. Ovako velike vrijednosti odabrane su zbog nestacionarnosti
samoigre: gradijenti procijenjeni na malom uzorku u kompetitivnom okruženju izrazito su bučni jer
se distribucija stanja mijenja zajedno s protivnikom.

Stopa učenja `learning_rate` iznosi 3 · 10⁻⁴ uz linearni raspored, što znači da tijekom pokusa
ravnomjerno pada prema nuli i do kraja proračuna koraka iščezava. Isti linearni raspored ima i
`epsilon`, parametar odsijecanja omjera vjerojatnosti u funkciji cilja, s početnom vrijednošću 0,2;
time se dopuštena promjena politike po ažuriranju postupno sužava kako treniranje odmiče.
Koeficijent regularizacije entropijom `beta` iznosi 5 · 10⁻³ i drži se konstantnim, čime se
istraživanje prostora akcija održava jednako poticanim do kraja pokusa umjesto da se gasi zajedno
sa stopom učenja. Parametar `lambd` iznosi 0,95 i određuje ravnotežu između pristranosti i
varijance u procjeni prednosti postupkom GAE.

## Arhitektura mreže

Blok `network_settings` opisuje mrežu koja je jednaka za oba agenta: dva skrivena sloja
(`num_layers` = 2) sa 256 jedinica (`hidden_units` = 256). Ta je veličina odabrana prema složenosti
prostora opažanja, koji uz vektor od osamnaest vrijednosti sadrži i zrake senzora
`RayPerceptionSensor3D`. Postavka `normalize` uključena je, pa trener tijekom rada održava tekuću
procjenu srednje vrijednosti i standardne devijacije svake komponente opažanja i normalizira ulaz —
što je nužno jer se komponente razlikuju po redu veličine, od položaja u areni do jediničnih vektora
smjera. Vrijednost `vis_encode_type` navedena je radi potpunosti; budući da agenti ne koriste vizualna
opažanja, ona nema učinka.

## Signal nagrade i proračun koraka

Jedini signal nagrade jest vanjski (`extrinsic`), s punom težinom `strength` = 1,0, dakle bez
dodatnih unutarnjih signala poput znatiželje. Faktor umanjenja `gamma` iznosi 0,99, čemu odgovara
efektivni horizont planiranja od približno 1 / (1 − γ) = 100 koraka odlučivanja. S obzirom na to da
je epizoda ograničena na 400 koraka odlučivanja, agent pri toj vrijednosti sagledava otprilike
četvrtinu najdulje moguće epizode. Upravo je taj parametar predmet zasebnog pokusa opisanog u
poglavlju o pretraživanju vrijednosti γ.

Proračun `max_steps` iznosi 5 000 000 koraka po ponašanju. Parametar `time_horizon` = 256 određuje
duljinu odsječka iskustva nakon kojega se procjena vrijednosti koristi za nastavak računa, pa se
duga epizoda dijeli na više takvih odsječaka umjesto da se čeka njezin završetak. Zapisivanje
mjernih veličina odvija se svakih `summary_freq` = 50 000 koraka, što tijekom cijelog pokusa daje
točno sto točaka po krivulji — upravo onoliko koliko ih sadrže grafovi prikazani u poglavlju o
rezultatima. Kontrolne točke spremaju se svakih `checkpoint_interval` = 250 000 koraka, dakle
dvadeset puta po pokusu, a `keep_checkpoints` = 20 osigurava da se nijedna od njih ne izbriše; time
je omogućeno naknadno vraćanje na raniju fazu treniranja ili nastavak prekinutog pokusa.

## Samoigra i parametar okoline

Blok `self_play` upravlja mehanizmom snimaka opisanim u potpoglavlju o samoigri i ovdje se ne
ponavlja; vrijednosti su `save_steps` = 50 000, `swap_steps` = 50 000, `team_change` = 100 000,
`window` = 10, `play_against_latest_model_ratio` = 0,5 i `initial_elo` = 1200.

Na kraju datoteke stoji blok `environment_parameters` s jedinom vrijednošću
`distance_shaping_coef` = 0,5. Za razliku od svega prethodnog, taj se parametar ne odnosi na trener
nego ga ML-Agents prosljeđuje okolini na početku svake epizode, gdje ga skripta agenta očitava i
prema njemu uključuje potencijalno oblikovanje nagrade. Rijetka ruka koristi istu datoteku s
vrijednošću 0,0. Postavljanjem odabira ruke u konfiguraciju, a ne u polje Unity Editora, sam je
odabir postao dio zapisa pokusa: iz datoteke `configuration.yaml` u mapi rezultata naknadno se može
utvrditi koja je ruka pokrenuta, bez oslanjanja na bilješke.

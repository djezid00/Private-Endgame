# 4. Implementacija

## 4.1 Arhitektura sustava

Implementacija je podijeljena u tri odvojene cjeline s jasno razgraničenim odgovornostima: klasa
`TagAgent` (opažanja, akcije i nagrada na razini pojedinačnog agenta), klasa `TagArenaManager`
(orkestracija arene — resetiranje, brojanje koraka, razrješavanje ishoda epizode, MA-POCA grupe) i
statički modul `TagReward` (čista, bez-stanja matematika nagrade). Odvajanje `TagReward` logike u
zaseban programski sklop (`TagGame.Reward`) omogućuje njezino jedinično testiranje neovisno o Unity
scenu i način izvođenja igre (engl. *Play mode*) — bitno svojstvo za komponentu koja izravno
određuje ispravnost eksperimentalne usporedbe opisane u poglavlju 3.5.

## 4.2 Agent — klasa `TagAgent`

`TagAgent` nasljeđuje ML-Agents klasu `Agent` i implementira ponašanje zajedničko objema ulogama, uz
grananje po `teamId` (0 = Lovac, 1 = Bjegunac) gdje se uloge razlikuju.

**Resetiranje epizode.** Metoda `OnEpisodeBegin()` poziva `arena.ResetArena()` isključivo kada je
`teamId == 0` (Lovac). Rana inačica u kojoj su oba agenta pozivala resetiranje sadržavala je utrku
stanja (engl. *race condition*): oba bi agenta postavila nove, međusobno neovisne pozicije, što je
povremeno dovodilo do trenutnog sudara na spawn poziciji, trenutnog završetka epizode i beskonačne
petlje resetiranja. Rješenje — da resetiranje provodi isključivo jedan agent — uklanja izvor utrke
stanja.

**Prostor opažanja.** Metoda `CollectObservations()` sastavlja 18 vrijednosti u dvije skupine od po
devet: vlastita lokalna pozicija, linearna brzina i orijentacija (`transform.forward`) agenta, te
relativna pozicija, brzina i orijentacija protivnika. Relativna (a ne apsolutna) pozicija protivnika
čini opažanje neovisnim o apsolutnom položaju u areni — agent uči obrazac poput "protivnik je 3
jedinice desno", a ne pamti apsolutne koordinate arene.

**Prostor akcija i nagrada po koraku.** Metoda `OnActionReceived()` prima dvije kontinuirane
vrijednosti (kretanje i skretanje), ograničene na raspon [-1, 1], te ih primjenjuje kroz
`Rigidbody.MovePosition` (kretanje) i rotaciju oko okomite osi (skretanje), obje skalirane s
`Time.fixedDeltaTime` radi neovisnosti o broju sličica u sekundi. U istoj metodi dodjeljuje se
nagrada po koraku ovisno o ulozi: Lovac prima kaznu −0,001 (§3.4) te, kada je koeficijent
oblikovanja različit od nule, dodatnu PBS nagradu izračunatu pozivom `TagReward.ShapingDelta`
(§4.4); Bjegunac prima nagradu +0,001. Isključivo Lovac poziva `arena.Step()`, čime se izbjegava
dvostruko brojanje koraka koje bi se dogodilo kad bi oba agenta ticala isti brojač.

**Detekcija sudara.** `OnCollisionEnter()` reagira samo na sudare s objektima označenima oznakom
`"Agent"` (izbjegava reakciju na zidove ili pod) te delegira odluku o ishodu klasi
`TagArenaManager.OnAgentTagged()`.

**Ručno upravljanje.** Metoda `Heuristic()` omogućuje ručno upravljanje agentom tipkovnicom (W/A/S/D)
bez pokrenutog trenera — korišteno za funkcionalno testiranje kretanja i fizike prije pokretanja
treninga.

## 4.3 Orkestracija arene — klasa `TagArenaManager`

**MA-POCA grupe.** U metodi `Start()` instanciraju se dva objekta `SimpleMultiAgentGroup` — jedan po
ulozi — i u svaki se registrira pripadajući agent (`RegisterAgent`). Ova podjela po ulogama, a ne
jedna zajednička grupa, preduvjet je da algoritam trenira kao MA-POCA [5] (§3.3); ujedno omogućuje
buduće proširenje na više agenata po timu (npr. drugi Lovac) dodavanjem još jednog poziva
`RegisterAgent`, bez izmjene mehanizma dodjele zasluga.

**Resetiranje arene.** `ResetArena()` postavlja Lovca na lijevu, a Bjegunca na desnu polovicu
kvadratne arene, uz nasumičnu poziciju i orijentaciju. Petlja s ograničenim brojem pokušaja
(`spawnRetryLimit = 30`) ponovno izvlači poziciju Bjegunca ako je udaljenost između agenata manja od
`minSpawnDistance` (3 jedinice), čime se sprječava sudar odmah po resetiranju. Preostala brzina iz
prethodne epizode eksplicitno se poništava na oba `Rigidbody` objekta.

**Brojač koraka i zastoj.** `Step()`, pozvan isključivo iz Lovčevog `OnActionReceived`, povećava
brojač koraka i pokreće `TriggerStalemate()` kada brojač dosegne `maxEpisodeSteps` (2000 fizikalnih
koraka, §3.1). Zastoj se tretira kao prekid (engl. *truncation*), ne kao istinski terminalni ishod:
poziva se `GroupEpisodeInterrupted()`, koji procjenu vrijednosti stanja premošćuje (engl.
*bootstrap*) na točki prekida, umjesto da je tretira kao stvaran kraj epizode.

**Razrješavanje hvatanja.** `OnAgentTagged()` dodjeljuje Lovčevoj grupi baznu nagradu +1 uvećanu za
vremenski bonus do +0,5 (brže hvatanje ⇒ veći bonus), a Bjegunčevoj grupi baznu kaznu −1 umanjenu
bonusom preživljavanja do +0,5 (dulje preživljavanje ⇒ manja neto kazna) — oba bonusa izračunata iz
udjela iskorištenog vremena epizode (`StepCount / MaxStep`). Budući da je hvatanje istinski
terminalni ishod, poziva se `EndGroupEpisode()` (bez premošćivanja procjene vrijednosti). Metrike
ishoda (`Environment/Catch`, `Environment/TimeToCatch`) bilježe se prije poziva `EndGroupEpisode()`,
jer taj poziv sinkrono pokreće Lovčev `OnEpisodeBegin → ResetArena`, koji brojač koraka postavlja na
nulu — obrnut redoslijed bio je uzrok pogreške u bilježenju `TimeToCatch` opisane u poglavlju 3.7.

## 4.4 Modul nagrade i jedinično testiranje — `TagReward`

Matematika potencijalnog oblikovanja nagrade (§3.4) izdvojena je u statičku, bez-stanja klasu
`TagReward`: `PlanarDistance` (udaljenost u XZ ravnini, zanemarujući visinu Y — agenti se kreću po
podu konstantne visine), `Potential` (funkcija `Φ`, s posebnim slučajem `coef = 0 ⇒ Φ ≡ 0` za rijetki
režim) i `ShapingDelta` (izračun `F = γΦ(s′) − Φ(s)`). Izdvajanje ove logike izvan `MonoBehaviour`
klasa omogućuje pet jediničnih testova (`TagRewardTests.cs`, EditMode) koji potvrđuju: udaljenost
zanemaruje os Y; potencijal je veći (manje negativan) za bliže pozicije; potencijal je identički nula
kada je `coef = 0` — čime je automatiziranim testom potvrđeno da je oblikovanje u rijetkom režimu
doista potpuno isključeno, a ne samo numerički zanemarivo; te da je `ShapingDelta` pozitivan pri
približavanju, a negativan pri udaljavanju. Ovi testovi daju formalnu potvrdu ispravnosti PBS
formule [8] neovisnu o promatranju treniranja, što je preduvjet za tvrdnju da su dva eksperimentalna
režima (§3.5) identična u svemu osim vrijednosti `coef`.

## 4.5 Konfiguracija treniranja i pokretanje

Treniranje pokreće ML-Agents Python alat `mlagents-learn` nad YAML konfiguracijom koja odvojeno
definira hiperparametre za `Chaser` i `Runner`: veličina serije (`batch_size` 2048), veličina
međuspremnika (`buffer_size` 40960), stopa učenja `3×10⁻⁴` s linearnim opadanjem, `beta = 5×10⁻³`,
`epsilon = 0,2` s linearnim opadanjem, `lambda = 0,95`, 5 epoha po ažuriranju, mreža s 256 skrivenih
jedinica u 2 sloja, te `extrinsic` signal nagrade s `gamma = 0,99` (mora odgovarati
`shapingGamma` u `TagAgent`, §3.4). Blok `self_play` određuje mehaniku samostalne igre: prozor od
10 spremljenih protivnika, 50% šanse igranja protiv najnovijeg modela, spremanje snimke i izmjena
timova svakih 50 000 koraka, s izmjenom uloge trenera (`team_change`) svakih 100 000 (dugi horizont)
odnosno 20 000 koraka (kratki horizont), uz početnu ELO ocjenu 1200.

Odabir eksperimentalnog režima (§3.5) proveden je isključivo preko jedne vrijednosti u konfiguraciji,
`environment_parameters.distance_shaping_coef` (0,0 za rijetki, 0,5 za gusti režim) — ne kroz izmjenu
koda ili ručno postavljanje polja u Unity Inspectoru — čime je odabir režima reproducibilan i
zapisan zajedno s ostatkom konfiguracije. Četiri konfiguracijske datoteke (kratki/dugi horizont ×
rijetki/gusti režim) arhivirane su u `experiments/configs/`. Točke provjere modela spremaju se svakih
250 000 koraka (`checkpoint_interval`), uz zadržavanje posljednjih 20 (`keep_checkpoints`), što
omogućuje nastavak prekinutog treninga (`--resume`).

Treniranje na dugom horizontu (5 000 000 koraka) provedeno je nad samostalno izgrađenom (engl.
*headless*) inačicom Unity aplikacije, pokrenutom bez grafičkog iscrtavanja (`--no-graphics`), čime
se procesorsko vrijeme oslobođeno od iscrtavanja preusmjerava na simulaciju i treniranje (§3.6).
Svih šest pokretanja dugog horizonta (2 režima × 3 sjemena) orkestrirano je jednom naredbenom
datotekom (`experiments/run_overnight_poca.bat`) koja ih izvodi neovisno, jedno za drugim, bez
nadzora korisnika.

## 4.6 Verzije i reproducibilnost

Implementacija koristi Unity 6000.4.0f1, Unity ML-Agents paket izdanje 23 i Python 3.12.4, s
PyTorch inačicom za CPU (§3.6). Svi modeli izvezeni su u `.onnx` format po završetku treninga, a
konfiguracije, sjemena slučajnosti i verzije alata arhivirani su zajedno s rezultatima radi
ponovljivosti eksperimenta.

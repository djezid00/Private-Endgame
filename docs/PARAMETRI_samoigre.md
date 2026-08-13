# Parametri samoigre — stvarne vrijednosti i objašnjenje

> Pomoćni dokument za pisanje §3.5 (samoigra) i §5 (eksperimentalni dizajn).
> Vrijednosti su očitane iz `results/<run>/configuration.yaml`, tj. iz onoga što je trener
> **stvarno koristio**, a ne iz konfiguracijskih datoteka kakve su danas na disku.
> Opisi parametara preuzeti su iz `ml-agents/docs/Training-Configuration-File.md`.

## 1. Provjera: jesu li ruke usporedive?

**Jesu.** Sve izvedene usporedbe koriste identične parametre samoigre unutar usporedbe.
Razlika u vrijednostima postoji, ali **između generacija pokusa**, ne između ruku:

| Generacija | `save_steps` | `swap_steps` | `team_change` | Pokretanja |
|---|---|---|---|---|
| Validacija, 400k | 25 000 | 25 000 | 50 000 | `TagVal_sparse_01`, `TagVal_shaped_01` |
| Glavni pokusi, 5M | 50 000 | 50 000 | 100 000 | `POCA_{sparse,shaped}_s{1,2,3}`, `PPO_{sparse,shaped}_s1`, `POCA_shaped_indivterm_s1`, svih 9 pokretanja γ-sweepa, obje γ-sonde |
| Smoke testovi | 5 000 | 3 000–5 000 | 20 000 | `TagTest_poca_01`, `*_smoke` |

Konstantno u **svim** pokretanjima: `window: 10`, `play_against_latest_model_ratio: 0.5`,
`initial_elo: 1200.0`.

Vrijednosti 25 000 / 25 000 / 50 000 nalaze se u datotekama `TagMApoca_sparse.yaml` **i**
`TagMApoca_shaped.yaml` — to su konfiguracije validacijskog pokusa na 400 000 koraka, i **obje
ruke** ih koriste. Datoteke `TagMApoca_sparse_5M.yaml` i `TagMApoca_shaped_5M.yaml` obje koriste
50 000 / 50 000 / 100 000. Zaključak: usporedba rijetke i oblikovane nagrade razlikuje se **samo**
po `distance_shaping_coef`, kako je i planirano.

> Napomena: `config/poca/TagMApoca.yaml` (50 000 / 30 000 / 200 000) je zatečeni predložak i
> **nije korišten ni za jedno pokretanje koje se navodi u radu**. Ne citirati ga kao izvor
> vrijednosti.

## 2. Što svaki parametar znači

- **`save_steps` = 50 000** — broj *trenerskih koraka* između dviju uzastopnih snimaka politike
  koje se spremaju u bazen protivnika. Veći iznos daje bazen protivnika koji pokriva širi raspon
  razina vještine i stilova igre, pa je naučena politika općenitija, ali je problem teži i traži
  više ukupnih koraka. Preporučeni raspon u dokumentaciji: 10 000 – 100 000.

- **`swap_steps` = 50 000** — broj *duhovnih koraka* (koraka agenta koji slijedi fiksnu politiku i
  ne uči) nakon kojih se protivniku zamjenjuje snimka. Razlikuje se od trenerskih koraka zbog
  asimetričnih igara s nejednakim brojem agenata po timu; u odnosu 1 na 1 prvi član formule
  jednak je 1, pa se broj zamjena unutar jednog razdoblja `team_change` dobiva jednostavnim
  dijeljenjem. Veći iznos znači dulju igru protiv istog fiksnog protivnika, dakle stabilnije ali
  manje raznoliko učenje.

- **`team_change` = 100 000** — broj *trenerskih koraka* nakon kojih se mijenja tim koji uči. Tim
  koji je učio prelazi u ulogu fiksnog protivnika i obrnuto. Dulje razdoblje znači da agent ima
  više vremena da nadigra trenutni skup protivnika, ali riskira prenaučenost na njihove konkretne
  strategije.

- **`window` = 10** — veličina kliznog prozora pohranjenih snimaka iz kojeg se uzorkuje protivnik.
  Pri svakoj novoj snimci najstarija se odbacuje. Veći prozor znači raznolikiji bazen protivnika,
  uključujući politike iz ranijih faza treniranja.

- **`play_against_latest_model_ratio` = 0.5** — vjerojatnost da protivnik bude *najnovija*
  politika. S vjerojatnošću 1 − 0,5 protivnik je snimka iz ranije iteracije, uzorkovana iz prozora.

- **`initial_elo` = 1200.0** — početna ELO ocjena obiju politika. Sve ELO vrijednosti u radu treba
  čitati kao odmak od te zajedničke polazne točke, a ne kao apsolutnu mjeru.

## 3. Izvedene veličine (za tekst rada)

Za glavne pokuse na 5 000 000 koraka po ponašanju:

| Veličina | Izračun | Iznos |
|---|---|---|
| Ukupno spremljenih snimaka | 5 000 000 / 50 000 | **100** |
| Vremenski raspon bazena protivnika | `window` × `save_steps` = 10 × 50 000 | **posljednjih 500 000 koraka** |
| Zamjena uloge tima koji uči | 5 000 000 / 100 000 | **50 puta po pokretanju** |
| Zamjena protivničke snimke unutar jednog razdoblja `team_change` | 100 000 / 50 000 | **2 puta** |
| Udio epizoda protiv najnovije politike | `play_against_latest_model_ratio` | **50 %** |

Za validacijski pokus na 400 000 koraka: 16 snimaka, bazen pokriva posljednjih 250 000 koraka,
8 zamjena uloge, 2 zamjene snimke po razdoblju.

Omjer `team_change` / `save_steps` iznosi **2** u obje generacije — dosljedno, ali **ispod
preporučenog raspona 4–10×** iz dokumentacije ML-Agentsa (zadana vrijednost je `5 × save_steps`).
Praktična posljedica je da se uloga tima koji uči mijenja češće nego što zadana heuristika
predlaže, pa svaki tim ima kraće razdoblje da nadigra trenutni skup protivnika. To treba
**eksplicitno navesti** u ograničenjima (§6.5), a ne prešutjeti: odabir je bio dosljedan kroz sve
pokuse, pa ne ugrožava usporedivost ruku, ali jest odmak od preporuke.

## 4. Dvije napomene iz dokumentacije koje su izravno relevantne za nalaze rada

**(a) Preporuka za rijetku nagradu.** Dokumentacija ML-Agentsa uz odjeljak o samoigri izričito
savjetuje: *„encourage users to begin with the simplest possible reward function (+1 winning, −1
losing) and to allow for more iterations of training to compensate for the sparsity of reward"* —
uz obrazloženje da oblikovanje nagrade treba biti konzervativno zbog nestabilnosti i
nestacionarnosti učenja u kompetitivnim igrama. Nalaz rada (rijetka nagrada nadmašuje oblikovanu
na 5M koraka) time nije samo empirijski rezultat nego i **potvrda dokumentirane preporuke**, što
je jača formulacija za raspravu.

**(b) Pretpostavka na kojoj počiva ELO.** Dokumentacija navodi: *„We make the assumption that the
final reward in a trajectory corresponds to the outcome of an episode. A final reward of +1
indicates winning, −1 indicates losing and 0 indicates a draw. The ELO calculation depends on this
final reward being either +1, 0, −1."* U oblikovanoj ruci završna nagrada trajektorije nije čisti
ishod ±1 jer joj se pribraja oblikovanje, pa je ta pretpostavka narušena. To **ne poništava** ELO
kao mjeru, ali ga u oblikovanoj ruci čini manje izravno interpretabilnim — dodatan razlog zašto se
ruke uspoređuju po grupnoj nagradi i stopi hvatanja, a ne po individualnoj nagradi. Vrijedi
navesti u §6.5 uz ostala ograničenja.

## 5. Tekst za popunjavanje praznina u §3.5

Nacrt §3.5 (`NACRT_3.5_samoigra_i_5.2_mjerne_velicine.md`, redak 67) ima praznine
⟨`save_steps`⟩, ⟨`swap_steps`⟩, ⟨`window`⟩, ⟨`play_against_latest_model_ratio`⟩ i ⟨`team_change`⟩.
Prijedlog teksta:

> U konfiguraciji korištenoj u glavnim pokusima ovog rada te vrijednosti iznose `save_steps` =
> 50 000, `swap_steps` = 50 000, `window` = 10, `play_against_latest_model_ratio` = 0,5 i
> `team_change` = 100 000, uz početnu ocjenu `initial_elo` = 1200. Tijekom pokretanja od 5 000 000
> koraka to znači 100 spremljenih snimaka politike, bazen protivnika koji u svakom trenutku
> pokriva posljednjih 500 000 koraka treniranja, 50 izmjena tima koji uči te podjednak omjer
> epizoda odigranih protiv najnovije politike i protiv ranijih snimaka. U validacijskom pokusu na
> 400 000 koraka korištene su proporcionalno manje vrijednosti (`save_steps` = `swap_steps` =
> 25 000, `team_change` = 50 000), pri čemu su obje uspoređivane ruke koristile identične
> postavke, tako da se ruke razlikuju isključivo po koeficijentu oblikovanja nagrade.

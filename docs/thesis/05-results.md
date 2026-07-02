# 5. Rezultati

## 5.1 Potvrda ispravnosti MA-POCA implementacije

Prije analize eksperimentalnih rezultata potvrđeno je da implementirani sustav zaista provodi
MA-POCA dodjelu zasluga, a ne neovisno PPO učenje [7]. Dokaz je zabilježen na probnom pokretanju od
50 000 koraka (`TagTest_poca_01`): uz `Policy/PolicyLoss` (0,0176 Lovac / 0,0154 Bjegunac) i
`Policy/ValueLoss` (0,0202 / 0,0201), zabilježen je konačan `Policy/BaselineLoss` (0,0202 / 0,0206).
Gubitak bazne linije javlja se isključivo kada se optimizira kontrafaktualna bazna linija MA-POCA
kritičara [5]; kod neovisnog PPO učenja ovaj gubitak se ne pojavljuje. Prisutnost konačnog,
nenultog `BaselineLoss` uz `ValueLoss` stoga je izravan dokaz da implementacija provodi pravu
MA-POCA dodjelu zasluga.

Dodatni pokazatelji potvrđuju ispravnost sustava: procjene vrijednosti stanja ispravno su usmjerene
(`ExtrinsicValueEstimate` ≈ −0,229 za Lovca, +0,042 za Bjegunca, u skladu s time da kritičar već
predviđa poraz Lovca); timska nagrada čisto se raspoređuje po timovima
(`GroupCumulativeReward`: Lovac −0,66, Bjegunac +0,85); ELO ocjena i mehanizam izmjene timova
(`team_change`) funkcioniraju kako je predviđeno.

## 5.2 Karakterizacija početne, nasumične politike

Pri 50 000–60 000 koraka politike su još gotovo nasumične, što definira početni režim iz kojeg
proces učenja mora izaći. Prosječna duljina epizode iznosi ≈ 393/380 koraka odluke, blizu gornje
granice od 400 koraka — velika većina epizoda završava istekom vremena (zastojem), a ne hvatanjem.
Procijenjena stopa uspješnog hvatanja iznosi ≈ 5–15%. Ova mjerenja služe kao referentna točka za
usporedbu s rezultatima nakon treniranja.

## 5.3 Rezultati na kratkom horizontu treniranja (400 000 koraka)

Oba režima (rijetka i gusta/PBS nagrada) trenirana su 400 000 koraka, uz isto sjeme (12345) i 8
arena, uz identične parametre osim člana oblikovanja udaljenosti za Lovca.

| Metrika (Lovac, osim gdje je naznačeno) | Rijetka nagrada | Gusta (PBS) nagrada |
|---|---|---|
| ELO — Lovac | 1212,6 | 1236,4 |
| ELO — Bjegunac | 1190,7 | 1163,7 |
| Razlika ELO (Lovac−Bjegunac) | +21,9 | +72,7 |
| Stopa uspješnog hvatanja | ≈0,08 | ≈0,21 |
| Duljina epizode (koraci) | 386 | 374 |
| Kumulativna timska nagrada — Lovac | −0,91 | −0,75 |
| Kumulativna timska nagrada — Bjegunac | +0,94 | +0,86 |
| Entropija politike | ≈1,43 | ≈1,43 |

Oba režima pokazuju napredak Lovca u odnosu na nasumičnu osnovicu (rast ELO ocjene i stope
hvatanja, pad duljine epizode), što potvrđuje da i isključivo rijetka, terminalna nagrada dovodi do
početka učenja potjere. Gusta PBS nagrada pritom ubrzava učenje: razlika ELO ocjene je približno
3 puta veća, a stopa hvatanja 2,5–3 puta veća nego kod rijetke nagrade, uz nižu duljinu epizode.
Kumulativna timska nagrada — jedina mjera neovisna o dodatnom PBS članu — pokazuje da gusti režim
ostvaruje stvarno više pobjeda (−0,75 naspram −0,91), ne samo veće brojčane vrijednosti nagrade.
Entropija politike ostaje visoka u oba režima (≈1,43), što pokazuje da nijedna politika još nije
konvergirala pri ovom horizontu treniranja.

![Pregled svih zabilježenih metrika, rijetka naspram guste nagrade](../figures/validation/tb_overview.png)

*Slika 5.1 — Pregled svih zabilježenih TensorBoard metrika za oba režima na 400 000 koraka
(izglađivanje 0,8; plava/cijan = Lovac, crvena/roza = Bjegunac; svjetlije nijanse = gusta nagrada).*

![Divergencija ELO ocjene u samostalnoj igri](../figures/validation/tb_elo.png)

*Slika 5.2 — `Self-play/ELO` za oba režima. Oba se odmiču od početne vrijednosti 1200 u suprotnim
smjerovima za Lovca i Bjegunca; kod gustog režima razmak je izraženiji (Lovac ~1236, Bjegunac
~1164).*

![Stopa hvatanja i duljina epizode](../figures/validation/tb_catch_episodelen.png)

*Slika 5.3 — `Environment/Catch` (stopa hvatanja) i `Environment/EpisodeLength` (posredna mjera
brzine hvatanja). Gusti režim održava višu stopu hvatanja i nižu duljinu epizode tijekom cijelog
treninga.*

![Dijagnostika politike i kritičara](../figures/validation/tb_policy.png)

*Slika 5.4 — Entropija ostaje visoka (~1,40–1,43) za oba režima; procjene vrijednosti
(`ExtrinsicValueEstimate`) i bazne linije (`ExtrinsicBaselineEstimate`) divergiraju u očekivanom
smjeru (Lovac negativno, Bjegunac pozitivno); stopa učenja i `epsilon` opadaju prema rasporedu.*

## 5.4 Rezultati na dugom horizontu treniranja (5 000 000 koraka)

Nakon 5 000 000 koraka po ponašanju (16 arena, headless build, 3 sjemena po režimu), rangiranje
dobiveno na kratkom horizontu (§5.3) se preokreće.

| Horizont | Rijetka nagrada | Gusta (PBS) nagrada |
|---|---|---|
| 400 000 koraka | Lovac ispred ~+22 ELO | Lovac ispred ~+73 ELO |
| 5 000 000 koraka | Lovac dominira (ELO ≈1890 naspram ≈670; timska nagrada ≈+1,45) | Lovac gubi (timska nagrada ≈−0,98 do −1,00; ELO ≈1250–1320) |

Podaci po sjemenu (očitano iz konzolnih zapisa treniranja; vidi ograničenje u §6.4):

| Pokretanje (rijetka nagrada) | Prikazani tim | ELO | Kumulativna timska nagrada |
|---|---|---|---|
| `sparse_s1` | Lovac | 1890,7 | +1,45 |
| `sparse_s2` | Bjegunac | 685,5 | −0,87 |
| `sparse_s3` | Bjegunac | 661,1 | −0,94 |

| Pokretanje (gusta/PBS nagrada) | Prikazani tim | ELO | Kumulativna timska nagrada | Pojedinačna nagrada (uklj. PBS) |
|---|---|---|---|---|
| `shaped_s1` | Lovac | 1252,0 | −0,98 | 5,38 |
| `shaped_s2` | Lovac | — | −1,00 | 3,93 |
| `shaped_s3` | Lovac | 1317,7 | −0,96 | 4,29 |

U režimu s rijetkom nagradom Lovac pobjeđuje u sva tri sjemena. U režimu s gustom (PBS) nagradom
Lovac gubi u sva tri sjemena, uz upadljivu kombinaciju: kumulativna timska nagrada blizu −1 (stvaran
poraz), dok je pojedinačna nagrada (koja uključuje PBS član) istovremeno visoka (3,9–5,4).

Ova kombinacija — visoka pojedinačna nagrada uz timsku nagradu blizu −1 — pokazuje da Lovac u gustom
režimu maksimizira gustu potencijalnu nagradu zadržavanjem blizine Bjegunca, bez stvarnog
dovršavanja hvatanja — lokalni optimum koji se ne napušta ni nakon 5 000 000 koraka samostalne igre.
Lovac u rijetkom režimu nema takav alternativni izvor nagrade te je prisiljen naučiti stvarno
presretanje.

## 5.5 Kvalitativno promatranje ponašanja (u tijeku)

Kao dopuna brojčanim pokazateljima, planirano je izravno promatranje istreniranih modela unutar
Unity uređivača (način rada *Inference Only*, sparivanje `Chaser.onnx` i `Runner.onnx` iz istog
pokretanja) radi vizualne potvrde opisanog obrasca — očekuje se da rijetki Lovac aktivno progoni i
hvata Bjegunca, dok gusti Lovac kruži oko Bjegunca bez namjere dovršavanja hvatanja. U trenutku
pisanja ovo promatranje je planirano, ali još nije formalno zabilježeno kao provedeno (§6.4); brojčani
nalazi u §5.4 ne ovise o njemu, ali njegovo dovršavanje ojačalo bi nalaz neovisnim, kvalitativnim
dokazom.

## 5.6 Sažetak rezultata

Rangiranje dvaju režima dobiveno pri 400 000 koraka (gusta nagrada bolja) obrnuto je u odnosu na
rangiranje pri 5 000 000 koraka (rijetka nagrada bolja). Rijetka, isključivo terminalna nagrada
dovoljna je za pojavu ponašanja potjere pri dovoljno dugom horizontu treniranja, dok gusta PBS
nagrada, unatoč teorijskom jamstvu invarijantnosti optimalne politike [8], u ovom eksperimentu vodi
do lokalnog optimuma koji sprječava razvoj ciljanog ponašanja.

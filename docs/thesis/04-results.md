# 4. Rezultati

## 4.1 Potvrda ispravnosti MA-POCA implementacije

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

## 4.2 Karakterizacija početne, nasumične politike

Pri 50 000–60 000 koraka politike su još gotovo nasumične, što definira početni režim iz kojeg
proces učenja mora izaći. Prosječna duljina epizode iznosi ≈ 393/380 koraka odluke, blizu gornje
granice od 400 koraka — velika većina epizoda završava istekom vremena (zastojem), a ne hvatanjem.
Procijenjena stopa uspješnog hvatanja iznosi ≈ 5–15%. Ova mjerenja služe kao referentna točka za
usporedbu s rezultatima nakon treniranja.

## 4.3 Rezultati na kratkom horizontu treniranja (400 000 koraka)

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

## 4.4 Rezultati na dugom horizontu treniranja (5 000 000 koraka)

Nakon 5 000 000 koraka po ponašanju (16 arena, headless build, 3 sjemena po režimu), rangiranje
dobiveno na kratkom horizontu (§4.3) se preokreće.

| Horizont | Rijetka nagrada | Gusta (PBS) nagrada |
|---|---|---|
| 400 000 koraka | Lovac ispred ~+22 ELO | Lovac ispred ~+73 ELO |
| 5 000 000 koraka | Lovac dominira (ELO ≈1890 naspram ≈670; timska nagrada ≈+1,45) | Lovac gubi (timska nagrada ≈−0,98 do −1,00; ELO ≈1250–1320) |

U režimu s rijetkom nagradom Lovac pobjeđuje u sva tri sjemena: `sparse_s1` postiže ELO 1890,7 uz
timsku nagradu +1,45; `sparse_s2` i `sparse_s3` pokazuju odgovarajući pad ELO ocjene Bjegunca (685,5
i 661,1) uz timsku nagradu −0,87 i −0,94. U režimu s gustom (PBS) nagradom Lovac gubi u sva tri
sjemena: timska nagrada iznosi −0,98, −1,00 i −0,96, dok je pojedinačna nagrada (koja uključuje PBS
član) istovremeno visoka (5,38, 3,93, 4,29).

Kombinacija visoke pojedinačne nagrade i timske nagrade blizu −1 pokazuje da Lovac u gustom režimu
maksimizira gustu potencijalnu nagradu zadržavanjem blizine Bjegunca, bez stvarnog dovršavanja
hvatanja — lokalni optimum koji se ne napušta ni nakon 5 000 000 koraka samostalne igre. Lovac u
rijetkom režimu nema takav alternativni izvor nagrade te je prisiljen naučiti stvarno presretanje.

## 4.5 Sažetak rezultata

Rangiranje dvaju režima dobiveno pri 400 000 koraka (gusta nagrada bolja) obrnuto je u odnosu na
rangiranje pri 5 000 000 koraka (rijetka nagrada bolja). Rijetka, isključivo terminalna nagrada
dovoljna je za pojavu ponašanja potjere pri dovoljno dugom horizontu treniranja, dok gusta PBS
nagrada, unatoč teorijskom jamstvu invarijantnosti optimalne politike [8], u ovom eksperimentu vodi
do lokalnog optimuma koji sprječava razvoj ciljanog ponašanja.

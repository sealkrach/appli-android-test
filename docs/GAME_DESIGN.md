# PolitiRush — Document de conception

> Jeu mobile hyper-casual, volontairement débile, satirique et addictif.
> Un héros de film d'action arrose de tomates des hordes de caricatures politiques
> en franchissant des portes qui multiplient ses tirs.

## 1. Pitch en une phrase

Tu es un pastiche de héros d'action, tu descends une piste infinie, tu choisis à chaque
paire de portes celle qui gonfle ton tir (x2, +5, x3) en évitant les pièges (-3, ÷2),
et tu dois tomater les caricatures avant qu'elles n'arrivent jusqu'à toi.

## 2. Références de genre

Le genre "runner à portes multiplicatrices" domine les tops hyper-casual depuis 2023 :
défilement vertical, personnage en bas, doigt qui glisse à gauche ou à droite, portes
colorées vert/rouge avec une opération dessus, hordes d'ennemis, boss, effets de
nombres qui explosent à l'écran. Ce que PolitiRush garde de la recette :

- **Une seule entrée** : glisser le doigt. Un bouton "spécial" en plus, c'est tout.
- **La tentation des portes** : toujours une bonne porte, souvent une mauvaise à côté,
  parfois deux bonnes dont l'une est nettement meilleure.
- **Le nombre qui grossit** : le multiplicateur de tir s'affiche gros sous le héros ;
  c'est LE feedback qui donne envie de continuer.
- **Le boss toutes les 5 vagues** : pic de tension, grosse récompense.
- **La méta-progression** : les pièces achètent des améliorations permanentes,
  donc chaque partie perdue sert quand même à quelque chose ("encore une !").

## 3. Cadrage contenu (important)

1. **Héros : pastiches, pas de marques.** Les héros sont inspirés de personnages
   de films d'action, mais portent des noms fictifs et des traits reconnaissables
   (bandeau rouge, boxeur de Philadelphie...). Aucun nom déposé n'apparaît.
2. **Cibles : personnalités publiques en caricature.** Choix assumé du projet :
   les ministres et les boss sont des personnes réelles, dessinées en caricature
   reconnaissable (coiffure, costume, accessoire signature). La caricature de
   personnalités publiques relève de la satire, mais elle expose à deux risques
   à garder en tête avant publication : le droit à l'image (en France, la
   satire de personnalités publiques est largement protégée mais pas illimitée)
   et la politique Google Play sur le contenu visant des personnes réelles.
   Le catalogue est purement des données (`Roster.kt`) : remplacer un nom réel
   par un archétype est une ligne à changer.
3. **Aucune violence réaliste.** Les projectiles sont des tomates, quelle que soit
   l'arme (main, lance-pierre, canon, tank). Les cibles "éliminées" s'écrasent
   façon cartoon (splash rouge, étoiles, bruit de ventouse). Pas de sang.
   Classification visée : PEGI 12 (satire politique).
4. **Pas de cliché complotiste.** Le rang « hyper-influent » regroupe des
   dirigeants et des patrons de la tech dont l'influence mondiale est factuelle.
   Il ne doit pas servir à mettre en scène des « marionnettistes de l'ombre ».

## 4. Les héros

| Id | Nom en jeu | Inspiration (interne) | Cadence | Dégâts | Spécial |
|---|---|---|---|---|---|
| `rambeau` | Jean Rambeau | Rambo | 6/s | 1 | **Rafale** : cadence x3 pendant 4 s |
| `balbo` | Rocco Balbo | Rocky | 3/s | 3 | **Uppercut** : onde de choc, 10 dégâts et recul autour du héros |
| `machete` | El Machette | Machete | 4/s | 2 | **Lames** : projectiles perçants pendant 5 s |
| `chauve_souris` | Le Chevalier Chauve-Souris | Batman | 4.5/s | 1 | **Gadget** : gèle toutes les cibles 4 s |
| `papa_particulier` | Le Papa Très Particulier | Taken | 5/s | 2 | **Compétences très particulières** : tir en éventail sur toute la largeur 6 s |
| `transporteur` | Le Transporteur Chauve | Jason Statham | 5/s | 1 | **Livraison** : double le multiplicateur courant |

Idées de héros pour plus tard : le flic de Los Angeles en marcel, le pilote de
"Fast", l'astronaute-forreur qui sauve la Terre, le vieux tueur qui veut juste son
chien, l'archéologue au fouet. Toujours en pastiche.

Le premier héros est débloqué d'office ; les autres après 3 parties (placeholder :
à terme, déblocage par pièces ou par défi).

### Les armes suivent la puissance

Le multiplicateur de tir détermine l'arme affichée. La mécanique ne change pas
(N tomates par salve), mais le joueur voit son équipement grossir :

| Multiplicateur | Arme | Rendu |
|---|---|---|
| x1 à x4 | À la main | Le héros court et lance à la main |
| x5 à x12 | Lance-pierre | Lance-pierre en Y tenu devant |
| x13 à x32 | Canon à tomates | Canon sur roues à côté du héros |
| x33 à x64 | Tank à tomates | Le héros dépasse de la tourelle |

Le changement d'arme est annoncé en gros à l'écran (« CANON À TOMATES ! »),
c'est un des moments de satisfaction de la partie.

### Animation

Les personnages sont des humanoïdes dessinés (tête, buste, bras, jambes) avec un
cycle de course : jambes et bras en balancier, léger rebond. Le héros court quand
le joueur bouge, les cibles courent toujours vers lui. Voir `drawPerson` dans le
prototype web pour la référence du cycle.

## 5. Les cibles : la résistance suit le rang réel

Principe : plus la personne est haut placée, plus elle encaisse. Deux ministres
ont la même résistance quel que soit le bord politique.

| Rang | PV | Vitesse | Points | Pièces | Apparaît dès | Exemples |
|---|---|---|---|---|---|---|
| Haut fonctionnaire | 1 | rapide | 10 | 1 | vague 1 | Le Préfet, L'Inspecteur des Finances, La Directrice de Cabinet |
| Député | 2 | moyenne | 20 | 2 | vague 2 | Le Député de base, La Députée en marche, Le Député insoumis |
| Ministre | 5 | lente | 50 | 3 | vague 4 | Gabriel Attal, Aurélie Filippetti, Bruno Le Maire, Gérald Darmanin |
| Chef d'État (boss) | 60 à 80 | très lente | 500 à 600 | 25 à 30 | vagues 5 et 10 | Emmanuel Macron, Benyamin Netanyahou |
| Hyper-influent (boss final) | 120 à 160 | très lente | 1000 à 1300 | 50 à 60 | vague 15 et au-delà | Donald Trump, Elon Musk, Vladimir Poutine, Mark Zuckerberg |

Les rangs bas restent majoritaires même dans les vagues avancées (poids
d'apparition 4 / 2 / 1), les ministres sont des moments de tension, les boss
des événements. Toutes les cibles gagnent 1 PV toutes les 3 vagues.

Traits de caricature à produire (référence dans le prototype web, objet `look`) :
coiffure (courte, longue, carré, houppe, clairsemée, chauve), teint, costume,
cravate, lunettes, moustache, écharpe tricolore pour les élus, casquette rouge,
t-shirt sous la veste pour les patrons de la tech.

Gimmicks prévus (pas encore codés) : bouclier « rapport de 400 pages » pour les
hauts fonctionnaires, zigzag pour certains députés, pluie de tracts pour les chefs
d'État, portes ÷2 invoquées par les hyper-influents.

## 6. Les portes

- Une paire toutes les 6 secondes, une à gauche, une à droite.
- **Toujours au moins une bonne** (`+N` ou `xN`). Dans 65 % des cas l'autre est mauvaise
  (`-N` ou `÷2`). Sinon deux bonnes, le joueur doit lire vite laquelle est meilleure.
- Le multiplicateur est borné entre 1 et 64 (cap performance et lisibilité).
- Passer une porte affiche l'opération et le nouveau multiplicateur en gros au centre.

## 7. Les bonus (ramassables sur la piste)

| Bonus | Effet | Durée |
|---|---|---|
| Sondage | Tir en éventail sur toute la largeur | 5 s |
| Scandale | Toutes les cibles gelées | 3 s |
| Tomate géante | Projectiles perçants | 6 s |
| Motion de censure | Écran nettoyé (les boss perdent 30 %) | instantané |
| Meeting | Aimant à pièces | 8 s |

## 8. Score, combo, économie

- Score = points de la cible x multiplicateur de combo, où le multiplicateur vaut
  1 + (combo / 10). Le combo retombe après 2,5 s sans élimination.
- Pièces : lâchées à l'élimination, ramassées au contact (ou attirées par Meeting).
- Boutique (améliorations permanentes) : cadence, dégâts, multiplicateur de départ,
  vies, recharge du spécial. Coût x1,6 par niveau.

## 9. Boucle d'addiction, assumée

1. **Feedback immédiat** : nombre qui grossit, écran qui se remplit de tomates.
2. **Micro-décision permanente** : gauche ou droite, toutes les 6 secondes.
3. **Courbe de difficulté** : cadence d'apparition qui monte, PV qui montent avec les
   vagues, boss toutes les 5 vagues.
4. **Récompense même en cas d'échec** : les pièces restent, la boutique fait progresser.
5. **"Encore une"** : l'écran de fin a un seul gros bouton.
6. Plus tard : défi du jour (graine partagée : `Spawner(seed)` est déterministe),
   classement, héros à débloquer, skins de tomates.

## 10. Direction artistique (à produire)

- Cartoon plat, contours épais, palette : rouge tomate `#E63946`, orange `#F4A261`,
  vert d'eau `#2A9D8F`, fond nuit `#1B1B2F`.
- Personnages : humanoïdes à grosse tête, corps court, cycle de course à 4
  images. Cibles reconnaissables par coiffure, costume et accessoire signature.
- Sons : ventouse, splash, "ding" de porte, foule qui hue en boucle.
- Le rendu actuel (formes dessinées + texte) est un placeholder pensé pour être
  remplacé sprite par sprite sans toucher au moteur.

## 11. Monétisation (plus tard, pas dans le prototype)

Pub récompensée pour doubler les pièces de fin de partie ou continuer une fois,
interstitiel toutes les 3 parties, pack "sans pub". Rien de tout cela n'est câblé.

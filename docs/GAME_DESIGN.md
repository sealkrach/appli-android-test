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

Deux garde-fous, décidés dès le départ pour ne pas se faire retirer de Google Play
et ne pas s'exposer à des problèmes de droit à l'image ou de marques :

1. **Aucune personne réelle.** Les cibles sont des *archétypes* politiques
   caricaturés, jamais nommés. Les héros sont des *pastiches* de personnages de
   films : on reconnaît le bandeau rouge ou le boxeur de Philadelphie sans utiliser
   un nom déposé.
2. **Aucune violence réaliste.** Les projectiles sont des tomates. Les cibles
   "éliminées" s'écrasent façon cartoon (splash rouge, étoiles, bruit de ventouse).
   Pas de sang, pas d'arme réaliste. Classification visée : PEGI 7 / Everyone 10+.

Les inspirations de chaque personnage sont notées ci-dessous pour guider le
graphiste, mais elles ne doivent apparaître nulle part dans le jeu.

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

## 5. Les cibles

| Nom | PV | Vitesse | Points | Pièces | Gimmick prévu |
|---|---|---|---|---|---|
| Le Promettologue | 2 | lente | 10 | 1 | Bulle "promis juré" qui éclate |
| La Girouette | 1 | rapide | 15 | 1 | Change de couloir en zigzag |
| Le Baron Local | 4 | lente | 25 | 2 | Écharpe tricolore, se fend en deux |
| Le Technocrate | 3 | moyenne | 20 | 2 | Bouclier "rapport de 400 pages" (1 salve absorbée) |
| L'Influenceur Populiste | 2 | rapide | 20 | 2 | Se duplique à la mort (deux mini-clones) |
| Le Dinosaure du Sénat | 6 | très lente | 40 | 3 | Tank, ralentit encore plus quand il est touché |

Boss (toutes les 5 vagues) :

- **Le Candidat Éternel** (60 PV) : revient au centre après chaque coup, lâche une
  pluie de tracts qui masquent l'écran.
- **Le Ministre des Réformes Indispensables** (90 PV) : invoque des portes ÷2 devant lui.

Les gimmicks ne sont pas encore codés : le moteur gère PV, vitesse, points, pièces et
boss. Voir la roadmap.

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
- Cibles : têtes surdimensionnées, costume et un accessoire signature (écharpe,
  rapport, smartphone, fossile).
- Sons : ventouse, splash, "ding" de porte, foule qui hue en boucle.
- Le rendu actuel (cercles + texte) est un placeholder pensé pour être remplacé
  sprite par sprite sans toucher au moteur.

## 11. Monétisation (plus tard, pas dans le prototype)

Pub récompensée pour doubler les pièces de fin de partie ou continuer une fois,
interstitiel toutes les 3 parties, pack "sans pub". Rien de tout cela n'est câblé.

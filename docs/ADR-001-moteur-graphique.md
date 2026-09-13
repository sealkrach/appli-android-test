# ADR-001 — Moteur graphique et style cartoon

## Contexte

Le rendu Compose Canvas du module `app` (formes simples) suffit pour valider les
règles, pas pour un jeu publié. Le style visé est un cartoon vectoriel : contours
épais, ombrage cel, grosses têtes, expressions, squash and stretch, éclaboussures,
onomatopées, secousses d'écran. Le prototype web (`docs/prototype-web.html`,
objet `Cartoon` et fonction `drawPerson`) est la référence de ce style : il est
jouable et sert de cahier des charges visuel.

## Options étudiées

| Option | Pour | Contre |
|---|---|---|
| **Compose Canvas + rendu vectoriel maison** (état actuel) | Aucune dépendance, même code que le prototype web, Kotlin partout | Pas d'atlas, pas d'éditeur de particules, animation squelettique à écrire soi-même, performance limitée au-delà de quelques centaines d'objets |
| **libGDX** (Kotlin/JVM, 2D) | Mature, gratuit, atlas de sprites, éditeur de particules, shaders (contour, cel), export Android et desktop depuis le même code, le module `core` se branche tel quel | Rendu vectoriel moins naturel (sprites bitmap), API bas niveau, communauté plus petite qu'Unity |
| **Rive** (animation vectorielle) + Compose | Cartoon vectoriel natif, animations d'état (course, coup reçu, mort) faites par le graphiste dans l'éditeur Rive, runtime Android et web gratuits | Moteur d'animation, pas moteur de jeu : la scène, les particules et la caméra restent à écrire |
| **Godot 4** | Moteur complet, 2D excellente, gratuit, export Android | Réécriture du moteur `core` en GDScript ou C#, on perd les tests Kotlin |
| **Unity** | Standard de l'hyper-casual, écosystème pub et analytics | C#, licence, réécriture totale, lourdeur pour un projet de cette taille |

## Décision

**libGDX pour le moteur de rendu, Rive pour les personnages.**

- libGDX porte la scène : piste en parallaxe, portes, projectiles (SpriteBatch,
  plusieurs milliers de tomates sans effort), particules (éditeur libGDX pour les
  éclaboussures), caméra avec secousses, shaders de contour et de cel shading.
- Rive porte les personnages : chaque héros et chaque cible est un fichier `.riv`
  avec une machine à états (`run`, `hit`, `frozen`, `ko`). Le graphiste produit
  les caricatures en vectoriel avec les traits listés dans `GAME_DESIGN.md` §5,
  sans toucher au code. Le runtime Rive s'intègre dans libGDX via une texture
  rendue hors écran, ou en superposition Android si plus simple au départ.
- Le module `core` ne change pas : libGDX consomme `GameState` et `GameEvent`
  exactement comme le prototype web.

Le module Compose `app` est conservé le temps de la transition comme rendu de
secours et écran de menus, puis remplacé par le lanceur libGDX Android.

## Conséquences

- Nouveaux modules Gradle : `gdx-core` (rendu, JVM pur, testable sans SDK),
  `gdx-desktop` (lanceur LWJGL pour itérer sur PC), `gdx-android` (lanceur
  Android, inclus seulement si un SDK est présent, comme `app` aujourd'hui).
- Le style est fixé par le prototype web : mêmes couleurs, mêmes proportions
  (tête ≈ 1,3 unité, buste 2 unités, contour ≈ 0,16 unité), mêmes effets.
- Les tests du moteur restent la source de vérité des règles ; le rendu ne
  contient aucune logique de jeu.

# PolitiRush

Jeu mobile Android hyper-casual et satirique : un pastiche de héros de film d'action
arrose de tomates des caricatures politiques fictives, en franchissant des portes
qui multiplient ses tirs (x2, +5, x3...) et en ramassant des bonus.

- Conception complète : [docs/GAME_DESIGN.md](docs/GAME_DESIGN.md)
- Plan de travail : [docs/ROADMAP.md](docs/ROADMAP.md)

## Structure

```
core/   Moteur de jeu en Kotlin pur (aucune dépendance Android), testé avec JUnit 5.
app/    Application Android (Jetpack Compose) : écrans, rendu Canvas, sauvegarde.
docs/   Conception et roadmap.
```

Le moteur est un pas de simulation pur : `GameEngine.step(dt, input)` transforme un
`GameState` immuable en un nouvel état et une liste de `GameEvent` (utile pour les
sons, vibrations et particules). Le tirage (`Spawner`) est déterministe pour une
graine donnée, ce qui permet des défis du jour partagés.

## Lancer les tests du moteur (sans SDK Android)

```
gradle :core:test
```

Sans SDK Android, `settings.gradle.kts` n'inclut que le module `core`.

## Compiler l'application

Ouvrir le dossier dans Android Studio (Ladybug ou plus récent). Le module `app` est
détecté dès qu'un SDK Android est configuré (`ANDROID_HOME` ou `local.properties`).
Le module `app` n'a pas encore été compilé : la première ouverture dans Android
Studio peut demander quelques corrections (voir la roadmap, étape 1).

## Cadrage contenu

Aucune personne réelle et aucun nom déposé n'apparaît dans le jeu : cibles et héros
sont des archétypes et des pastiches, les projectiles sont des tomates. Le détail est
dans le document de conception, section 3.

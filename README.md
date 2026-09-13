# PolitiRush

Jeu mobile Android hyper-casual et satirique : un pastiche de héros de film d'action
arrose de tomates des caricatures de personnalités politiques, en franchissant des
portes qui multiplient ses tirs (x2, +5, x3...) et en ramassant des bonus. L'arme
grossit avec la puissance (main, lance-pierre, canon, tank) et la résistance des
cibles suit leur rang réel (haut fonctionnaire, député, ministre, chef d'État,
hyper-influent).

- Conception complète : [docs/GAME_DESIGN.md](docs/GAME_DESIGN.md)
- Plan de travail : [docs/ROADMAP.md](docs/ROADMAP.md)
- Moteur graphique : [docs/ADR-001-moteur-graphique.md](docs/ADR-001-moteur-graphique.md)
- Maquettes jouables : `docs/prototype-web.html` (2D cartoon) et `docs/prototype-web-3d.html` (3D toon, trois niveaux)

## Structure

```
core/   Moteur de jeu en Kotlin pur (aucune dépendance Android), testé avec JUnit 5.
        Spécification de référence des règles.
Assets/, Packages/, ProjectSettings/
        Projet Unity (cible de publication), à la racine pour qu'Unity Hub l'ouvre depuis
        GitHub : moteur porté en C# avec ses tests, scripts de scène, outil de construction
        de scène. Voir docs/UNITY.md.
tools/CoreCheck/  Vérification du moteur C# sans Unity (dotnet run).
app/    Application Android Compose (premier prototype, conservée pour référence).
docs/   Conception, roadmap, décisions d'architecture, maquettes web jouables.
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

Les héros sont des pastiches sans nom déposé. Les ministres et les boss sont des
personnalités publiques en caricature, choix assumé qui demande une vérification
juridique avant publication. Les projectiles sont toujours des tomates. Le détail
est dans le document de conception, section 3.

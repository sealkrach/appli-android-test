# Roadmap

## Étape 0 — Préparation (fait)

- [x] Projet Gradle en deux modules : `core` (Kotlin pur, testé) et `app` (Android, Compose).
- [x] Moteur : portes et multiplicateurs, tir automatique, cibles, boss, bonus, combo,
  pièces, spéciaux de héros, fin de partie. 18 tests unitaires.
- [x] Catalogue : 6 héros pastiches, cibles classées par rang réel (haut fonctionnaire,
  député, ministre), boss chefs d'État puis hyper-influents.
- [x] Armes selon la puissance : main, lance-pierre, canon, tank.
- [x] Prototype web jouable (personnages animés en course) pour valider le feeling.
- [x] Rendu cartoon vectoriel dans le prototype : contours, cel shading, squash and
  stretch, expressions, éclaboussures, onomatopées, secousses, décor en parallaxe.
- [x] ADR-001 : libGDX pour la scène, Rive pour les personnages, puis révision 3D : Unity
  (Godot 4 en alternative), `core` Kotlin comme spécification et banc de tests.
- [x] Maquette 3D jouable (three.js, toon, ombres) : `docs/prototype-web-3d.html`.
- [x] Trois niveaux à thème : Le Palais, Carburant à 2 €, L'aéroport bradé.
- [x] Courbe de difficulté explicite (`Difficulty.kt`) : résistance par rang et par vague,
  densité et mélange des rangs par avancée ; reportée dans les deux maquettes.
- [x] Maquette 3D : post-traitement (bloom, FXAA, vignette), KO physiques, tomates en cloche.
- [x] Méta-progression : boutique d'améliorations, sauvegarde locale.
- [x] Écrans : accueil / choix du héros, jeu, fin de partie, boutique.
- [x] CI GitHub Actions sur le module `core`.

## Étape 1 — Prototype jouable (Android Studio)

- [ ] Ouvrir le projet dans Android Studio, compiler `app`, corriger les éventuelles
  erreurs de compilation Compose (le module `app` n'a pas pu être compilé sans SDK).
- [ ] Jouer 10 minutes et régler `GameConfig` (vitesses, intervalles, PV).
- [ ] Vibration + son sur `GatePassed`, `EnemyKilled`, `HeroHurt`.
- [ ] Particules "splash" à partir de `GameEvent.EnemyKilled`.

## Étape 2 — Contenu

- [ ] Sprites des héros, des armes et des cibles (voir `docs/GAME_DESIGN.md` §4, §5, §10).
- [x] Projet Unity créé (`unity/`) : moteur `core` porté en C# avec ses 30 tests NUnit,
  vérifiés en .NET 8 hors Unity (`unity/Tools/CoreCheck`).
- [x] Scripts de scène Unity : contrôleur à pas fixe, synchronisation état → objets,
  personnages en primitives, armes, portes, tomates, pièces, HUD, entrée tactile, caméra.
- [x] Outil « PolitiRush > Construire la scène de jeu » pour obtenir une scène jouable en un clic.
- [ ] Ouvrir dans Unity 2022.3 LTS, lancer, corriger les éventuelles erreurs de compilation
  des scripts Runtime et Editor (non compilés ici : il faut l'éditeur Unity).
- [ ] Basculer sur URP, Shader Graph toon, bloom et vignette.
- [ ] Menu niveau / héros et boutique dans Unity.
- [ ] Produire les personnages en Rive (machine à états run / hit / frozen / ko)
  à partir des traits du design doc.
- [ ] Vérification juridique des caricatures avant publication (§3 du design doc).
- [ ] Gimmicks des cibles (zigzag, bouclier, clone à la mort, tank).
- [ ] Patterns des boss.
- [ ] Déblocage des héros par pièces.

## Étape 3 — Rétention

- [ ] Défi du jour (graine partagée, classement local).
- [ ] Missions ("passe 10 portes x3", "fais un combo de 30").
- [ ] Skins de tomates.
- [ ] Sauvegarde DataStore + éventuellement Play Games.

## Étape 4 — Publication

- [ ] Icône, captures, fiche Play Store, politique de confidentialité.
- [ ] Pubs récompensées et interstitiels.
- [ ] Analytics (funnel : partie 1 → partie 3 → boutique → J1/J7).

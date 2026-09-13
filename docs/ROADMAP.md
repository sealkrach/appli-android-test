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
- [x] ADR-001 : libGDX pour la scène, Rive pour les personnages.
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
- [ ] Créer les modules `gdx-core` / `gdx-desktop` / `gdx-android` (ADR-001) et y
  porter la scène du prototype web (piste, portes, tomates, particules, caméra).
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

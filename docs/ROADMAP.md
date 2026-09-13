# Roadmap

## Étape 0 — Préparation (fait)

- [x] Projet Gradle en deux modules : `core` (Kotlin pur, testé) et `app` (Android, Compose).
- [x] Moteur : portes et multiplicateurs, tir automatique, cibles, boss, bonus, combo,
  pièces, spéciaux de héros, fin de partie. 18 tests unitaires.
- [x] Catalogue : 6 héros pastiches, 6 cibles, 2 boss.
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

- [ ] Sprites des héros et des cibles (voir `docs/GAME_DESIGN.md` §10).
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

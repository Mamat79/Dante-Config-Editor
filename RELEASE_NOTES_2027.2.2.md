# Dante Config Editor 2027.2.2

- Corrige la proposition repetee de mise a jour alors que la bonne version est
  deja installee : Windows et Mac lisent la version de l'application lancee.
- Corrige l'affichage technique 403 lorsque GitHub limite les requetes :
  la recherche manuelle explique quand reessayer et le demarrage reste silencieux.
- Respecte le delai du serveur et le conserve apres fermeture/reouverture.
  DCE reste utilisable hors ligne et ne pretend pas etre a jour si la
  verification n'a pas pu aboutir.
- Ajoute des tests de non-regression sur les versions et les limites reseau.
  Matrice, multicast, projets, reglages et licences sont preserves.

La 2027.2.1 est restee en brouillon ; la 2027.2.2 livre les deux corrections.
macOS : pas de signature Developer ID ni de notarisation. Test natif Apple
Silicon sur Codemagic ; architecture Intel verifiee, pas de test sur Intel physique.

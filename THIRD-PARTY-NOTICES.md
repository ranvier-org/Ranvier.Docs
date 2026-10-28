# Third-party notices

The theme configuration, navigation components, front-matter decoding, icons, CSS cascade and progressive JavaScript were adapted from Nacara.Theme.Default in [Nacara](https://github.com/MangelMaxime/Nacara), copyright 2021 Mangel Maxime, licensed under Apache-2.0. The upstream license notice is included in LICENSE-Nacara.txt and the complete license in LICENSE-Apache-2.0.txt. The source checkout was based on commit cf8936ddfb3e1b3ef58d35bf6ef8d6d824cf1137 and included its current local fixes on 2026-10-28.

This adaptation replaces Feliz markup with Ranvier.View server rendering, represents the stylesheet as Ranvier.Css rules, and packages ready-to-use JavaScript without a Node build. Nacara-related class names and data attributes are preserved for behavior compatibility.

The shared example compiler, map authoring/model/runtime and associated regression fixtures are adapted from ranvier-org/Ranvier's documentation infrastructure, copyright 2026 Shayan Habibi, under the MIT license supplied in LICENSE. The initial import preserved the local Ranvier documentation implementation based on acb01655854744217a2073d633bdf7dc9ab6b88f, with central-host dependency and workspace adaptations made on 2026-10-28.

Editable examples include Anime.js 4.5.0 by Julien Garnier, licensed under MIT. Its upstream LICENSE.md is copied beside the locally served runtime modules under assets/live-example/vendor/animejs/. Other packaged browser/compiler assets retain the notices supplied by their upstream packages.

---
title: Documentation authoring
description: Connect a library's docs to the common Ranvier documentation site.
---

# Documentation authoring

The central Ranvier documentation site reads documentation from registered repositories. Each library owns its prose and sample code. The common host owns rendering, the theme, navigation, example compilation, validation and publication.

Every imported repository gets the same core Ranvier, View, motion and CSS example environment. Authors do not need to rebuild the documentation infrastructure in each library. The theme is rendered on the server using Ranvier.View and styled with Ranvier.Css; normal documentation remains readable without JavaScript.

Start with [Quick start](quick-start.md), then [Connect a repository](repositories.md). Read the [configuration reference](configuration.md) for the exact contract and [sections and navigation](navigation.md) for custom presentation.

The guide also covers [links and assets](links-assets.md), [runnable examples](examples.md), [local preview](preview.md), [CI and publishing](ci-publishing.md), [migration](migration.md) and [troubleshooting](troubleshooting.md). [Architecture](architecture.md) explains how the pieces fit together and where changes belong.

## Library documentation

These links use source identities and are resolved by the central import, independently of the public hostname:

- [Reactive core](repo:core/index.md)
- [Views and motion](repo:view/index.md)
- [Typed CSS](repo:css/index.md)
- [Persistence](repo:persistence/index.md)
- [WebCore accessibility](repo:webcore/ACCESSIBILITY.md)

## Implementation versus migration

This host is prepared independently of the existing repository sites. Registering or building an import does not disable an old site or change a library release. Switching public URLs, removing duplicate workflows and configuring redirects are later migration tasks.

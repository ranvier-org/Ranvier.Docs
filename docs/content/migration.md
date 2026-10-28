---
title: Migration and rollback
---

# Migration and rollback

## Current boundary

The central implementation is built first. Existing repository documentation sites, Pages settings and publication workflows are not disabled as part of that work. Migration is a later operation once the central output and authoring experience are verified.

## Prepare each repository

Inventory its current pages, public URLs, sidebar groups, static assets, generated references, live examples and independent browser demos. Confirm which files are published docs and which are research/implementation records. A broad `content: "docs"` can be useful initially, but it should not accidentally publish private drafts or large diagnostic artifacts.

Add or review the optional manifest. Set an explicit landing page and intentional sections. Make sure every published page is listed or explicitly unlisted. Add exclusions for files that do not belong in the site. Declare extra sample projects and prebuilt browser bundles where needed.

## Preserve incoming links

Map each old public URL to its central destination. Relative filenames, collection prefixes and title-derived routes can change URLs. Review anchors as well as pages. Test navigation from another source instead of only opening the destination directly.

Update internal cross-repository links to `repo:` source links where practical. Full old-site URLs need an intentional update; the importer does not blindly rewrite all external links in prose or code samples.

Keep an explicit redirect map for URLs readers may have bookmarked, linked in issues or indexed. GitHub Pages static redirects generally require a redirect page at each old path or an external hosting rule; changing a menu link alone is not a redirect. Implement and test the actual chosen hosting mechanism during migration.

## Preview the coordinated source set

Acquire the accepted lock and run the complete central build. Compare source sections and sample behaviour with the old site. Check images, downloads, browser demo bases, edit links, search results, narrow navigation and language-specific code rendering.

Use an artifact preview or separate unpublished target before changing a public domain. A library's own release build should remain independent of the central docs migration.

## Cut over

Once the central destination and redirects are verified, update organisation/repository README links and badges, package metadata documentation URLs and navigation links. Change the old site's workflow only when its redirect/retirement behaviour is ready. Coordinate Pages environment/domain settings explicitly; those settings are not imported from a manifest.

Avoid two workflows racing to deploy unrelated artifacts to the same Pages destination. Keep the old site available for rollback until central publication has been checked against its expected source revisions.

## Future repository ergonomics

For a new library after migration, add docs under the standard layout and one source registration. Customise its manifest only when defaults are insufficient. The central host supplies the theme, common dependencies, runnable fences and publication pipeline. Authors should not need a private fork of Site.fs to add another guide section.

## Rollback the cutover

Retain the old workflow/settings record and the last known working central lock. If the central deployment fails, restore the previous public artifact/settings or the old site as appropriate. Revert source lock/configuration changes and rebuild rather than patch generated output. Keep redirects consistent with the restored destination.

## Completion evidence

A migration is finished when old URLs resolve as intended, new URLs and anchors work, source edit links return to the right repository, every shared example family works, publication is reproducible from the lock, and authors can preview local changes using the documented command. Removing the old workflow is the last cleanup step, not the acceptance criterion.

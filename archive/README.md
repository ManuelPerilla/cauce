# Cauce reference archive

This directory preserves the terminal prototype, its configuration tools and Windows packaging sources as technical reference. It is excluded from Cauce's maintained builds, CI checks and distributed desktop packages.

The current application is in [`src/Cauce.Desktop`](../src/Cauce.Desktop), with shared library behavior in [`src/Cauce.Core`](../src/Cauce.Core). Follow the [current documentation](../docs/README.md) for development, installation and releases.

The archived projects use the `Cauce.Terminal` namespace. Their scripts and installer sources are historical reference, not a supported installation channel. Do not use them to publish current Cauce releases or to configure the desktop application's accounts.

See the [terminal source map](docs/terminal.md) for the preserved components.

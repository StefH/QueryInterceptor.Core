rem https://github.com/StefH/GitHubReleaseNotes

SET version=1.0.8

GitHubReleaseNotes --output ReleaseNotes.md --skip-empty-releases --exclude-labels question invalid doc --version %version%

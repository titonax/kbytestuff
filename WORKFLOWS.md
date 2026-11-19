# Wagic Builder workflows

1. **Update from fork release** checks the latest stable upstream release daily or manually. When new source is available, it merges it into a separate branch while retaining this fork's workflows and opens a draft pull request. Review every changed file and resolve conflicts before merging the pull request yourself. If a conflict prevents the merge, the workflow fails and reports the files without changing master. Enabling GitHub Actions permission to create pull requests may be necessary in repository settings.
2. After reviewing and merging, run **Generate artifact** manually on master and provide the version already present in AssemblyInfo and the changelog. The workflow builds and tests offline and update-notifications variants, uploads Actions artifacts, and publishes both to a GitHub Release only if all tests pass. Existing tags pointing elsewhere are rejected.

**Build checks** runs on pushes and pull requests and is the reusable build called by Generate artifact. It does not publish a Release. The update-notifications executable checks this fork's Releases and notifies the user; it does not download or install updates.

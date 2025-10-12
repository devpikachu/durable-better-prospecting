set -e

pushd DurableBetterProspecting.Cake
dotnet run --project DurableBetterProspecting.Cake.csproj -- "$@"
popd

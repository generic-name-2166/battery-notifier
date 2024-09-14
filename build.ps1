# https://dev.to/guilhermerochas/interop-your-net-application-with-rust-nk2

cd "notifier-rs"
cargo test
cargo build --release
cd ..
dotnet publish -c Release

if (Test-Path -Path "build" -PathType Container) {
  Remove-Item -Path "build" -Recurse -Force
}

mkdir build
cp bin/Release/net8.0/win-x64/publish/* build

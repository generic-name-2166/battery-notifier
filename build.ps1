# https://dev.to/guilhermerochas/interop-your-net-application-with-rust-nk2

cd "notifier-rs"
cargo test
cargo build --release
cd ..
dotnet build -c Release

cp bin/Release/net8.0/win-x64/* build

use interoptopus::util::NamespaceMappings;
use interoptopus::{Error, Interop};

#[test]
#[cfg_attr(miri, ignore)]
fn bindings_csharp() -> Result<(), Error> {
    use interoptopus_backend_csharp::{Config, Generator};

    Generator::new(
        Config {
            class: "Notifier".to_string(),
            dll_name: "notifier_rs".to_string(),
            namespace_mappings: NamespaceMappings::new("BatteryNotifier"),
            ..Config::default()
        },
        notifier_rs::ffi_inventory(),
    )
    .write_file("bindings/csharp/Notify.cs")?;

    Ok(())
}

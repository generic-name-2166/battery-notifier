use interoptopus::{ffi_function, function, Inventory, InventoryBuilder};
use notify_rust::{error, Notification, Timeout};

fn notify_(percentage: u16) -> error::Result<()> {
    let message: &str = match percentage {
        ..=47 => "too low.\n Connect a charger",
        48..=57 => return Ok(()),
        58.. => "too high.\n Disconnect charger",
    };
    let body: String = format!("Battery percent {percentage} ") + message;

    Notification::new()
        .summary("Battery notification")
        .body(&body)
        .timeout(Timeout::Never)
        .show()
}

#[ffi_function]
#[no_mangle]
pub extern "C" fn notify(percentage: u16) {
    let result = notify_(percentage);

    if let Err(error) = result {
        eprintln!("{error}");
    };
}

pub fn ffi_inventory() -> Inventory {
    InventoryBuilder::new()
        .register(function!(notify))
        .inventory()
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_notification() -> error::Result<()> {
        notify_(40)
    }
}

#[macro_export]
macro_rules! ffgl_handler {
    ($handler:ty) => {
        // pub use $crate::conversions::*;
        // pub use $crate::FFGLHandler;

        #[no_mangle]
        #[allow(non_snake_case)]
        #[allow(unused_variables)]
        extern "C" fn plugMain(
            functionCode: u32,
            inputValue: $crate::conversions::FFGLVal,
            instanceID: *mut $crate::Instance<$handler>,
        ) -> $crate::conversions::FFGLVal {
            match $crate::conversions::Op::try_from(functionCode) {
                Ok(function) => {
                    $crate::logln!("Op::{function:?}");
                    $crate::default_ffgl_callback(function, inputValue, unsafe{ instanceID.as_mut() })
                },
                Err(err) => {
                    let err_text = format!("Received fnCode {functionCode:?}");
                    $crate::logln!("Failed to parse fnCode {functionCode}");
                    panic!();
                },
            }
        }

        #[no_mangle]
        #[allow(non_snake_case)]
        pub extern "C" fn SetLogCallback(logCallback: $crate::FFGLLogger) {
            unsafe { *$crate::loading_logger.write().unwrap() = Some(logCallback) };

            std::panic::set_hook(Box::new(|cause| {
                $crate::logln!("{}", cause);
            }));
        }
    };
}

// pub use ffgl_extern;
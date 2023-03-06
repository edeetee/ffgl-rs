#[macro_export]
macro_rules! ffgl_handler {
    ($render_ty:ty) => {
        pub use $crate::conversions::*;
        pub use $crate::FFGLHandler;

        #[no_mangle]
        #[allow(non_snake_case)]
        #[allow(unused_variables)]
        extern "C" fn plugMain(
            functionCode: u32,
            inputValue: FFGLVal,
            instanceID: *mut $crate::Instance<$render_ty>,
        ) -> FFGLVal {
            match Op::try_from(functionCode) {
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
        }
    };
}

// pub use ffgl_extern;
#[macro_export]
macro_rules! ffgl_handler {
    ($handler:ty) => {
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
                    // $crate::logln!("Op::{function:?}");
                    let result = std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| {
                        $crate::default_ffgl_callback(function, inputValue, unsafe {
                            instanceID.as_mut()
                        })
                    }));

                    match result {
                        Ok(result) => result,
                        Err(err) => {
                            $crate::logln!("PANIC AT FFGL C BOUNDARY: {:#?}", err);
                            $crate::SuccessVal::Fail.into()
                        }
                    }
                }
                Err(err) => {
                    let err_text = format!("Received fnCode {functionCode:?}");
                    $crate::logln!("ERR: UNKNOWN OPCODE {functionCode}");
                    $crate::SuccessVal::Fail.into()
                }
            }
        }

        #[no_mangle]
        #[allow(non_snake_case)]
        pub extern "C" fn SetLogCallback(logCallback: $crate::FFGLLogger) {
            unsafe { *$crate::LOADING_LOGGER.write().unwrap() = Some(logCallback) };

            std::panic::set_hook(Box::new(|cause| {
                $crate::logln!("{}", cause);
            }));
        }
    };
}

// pub use ffgl_extern;

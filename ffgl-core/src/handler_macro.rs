///Generate the ```extern "C" fn plugMain(op, input, instance)``` entry point
#[macro_export]
macro_rules! plugin_main {
    ($handler:ty) => {
        #[no_mangle]
        #[allow(non_snake_case)]
        #[allow(unused_variables)]
        extern "C" fn plugMain(
            functionCode: u32,
            inputValue: $crate::conversions::FFGLVal,
            instanceID: *mut $crate::handler::Instance<
                <$handler as $crate::handler::FFGLHandler>::Instance,
            >,
        ) -> $crate::conversions::FFGLVal {
            use $crate::conversions::*;
            match $crate::conversions::Op::try_from(functionCode) {
                Ok(function) => {
                    $crate::tracing::trace!("Op::{function:?}");
                    let result = std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| {
                        $crate::entry::default_ffgl_entry::<$handler>(
                            function,
                            inputValue,
                            unsafe { instanceID.as_mut() },
                        )
                    }));

                    match result {
                        Ok(result) => match result {
                            Ok(result) => result,
                            Err(err) => {
                                $crate::tracing::error!(
                                    target: "plugin_main",
                                    "ERROR in {function:?}: {:?}",
                                    err,
                                );
                                SuccessVal::Fail.into()
                            }
                        },
                        Err(err) => {
                            $crate::tracing::error!(target: "plugin_main", "PANIC AT FFGL C BOUNDARY: {:?}", err);
                            SuccessVal::Fail.into()
                        }
                    }
                }
                Err(err) => {
                    $crate::tracing::warn!(target: "plugin_main", "ERR: UNKNOWN OPCODE {functionCode}");
                    SuccessVal::Fail.into()
                }
            }
        }

        #[no_mangle]
        #[allow(non_snake_case)]
        pub extern "C" fn SetLogCallback(logCallback: $crate::log::FFGLLogger) {
            $crate::log::init_logger(logCallback);
        }
    };
}

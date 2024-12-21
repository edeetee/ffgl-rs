use crate::{
    conversions::FFGLVal,
    entry::default_ffgl_entry,
    handler::{self, FFGLHandler},
};

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
            $crate::plugin_main::handle_plugin_main::<$handler>(
                functionCode,
                inputValue,
                instanceID,
            )
        }

        #[no_mangle]
        #[allow(non_snake_case)]
        pub extern "C" fn SetLogCallback(logCallback: $crate::log::FFGLLogger) {
            $crate::log::init_logger(logCallback);
        }
    };
}

pub fn handle_plugin_main<H: FFGLHandler + 'static>(
    function_code: u32,
    input_value: FFGLVal,
    instance_id: *mut handler::Instance<H::Instance>,
) -> FFGLVal {
    use crate::conversions::*;
    let _span = tracing::span!(tracing::Level::TRACE, "plug", "fn" = function_code).entered();
    match Op::try_from(function_code) {
        Ok(function) => {
            tracing::trace!("Op::{function:?}");
            let result = std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| {
                default_ffgl_entry::<H>(function, input_value, unsafe { instance_id.as_mut() })
            }));

            match result {
                Ok(result) => match result {
                    Ok(result) => result,
                    Err(err) => {
                        tracing::error!(
                            target: "plugin_main",
                            "ERROR in {function:?}: {:?}",
                            err,
                        );
                        SuccessVal::Fail.into()
                    }
                },
                Err(err) => {
                    tracing::error!(target: "plugin_main", "PANIC AT FFGL C BOUNDARY: {:?}", err);
                    SuccessVal::Fail.into()
                }
            }
        }
        Err(_) => {
            tracing::warn!(target: "plugin_main", "ERR: UNKNOWN OPCODE {function_code}");
            SuccessVal::Fail.into()
        }
    }
}

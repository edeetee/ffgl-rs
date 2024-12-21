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
            $crate::entry::handle_plugin_main::<$handler>(functionCode, inputValue, instanceID)
        }

        #[no_mangle]
        #[allow(non_snake_case)]
        pub extern "C" fn SetLogCallback(logCallback: $crate::log::FFGLLogger) {
            $crate::log::init_logger(logCallback);
        }
    };
}

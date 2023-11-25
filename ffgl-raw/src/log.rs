use std::{
    ffi::{c_char, CString},
    sync::RwLock,
};

pub static mut LOADING_LOGGER: std::sync::RwLock<Option<FFGLLogger>> = RwLock::new(None);
pub type FFGLLogger = unsafe extern "C" fn(*const c_char) -> ();

pub fn inner_log(str: &str) {
    unsafe {
        if let Some(logger) = *LOADING_LOGGER.read().unwrap() {
            let str = &CString::new(str).unwrap();
            logger(str.as_ptr());
        } else {
            eprintln!("{}", str);
        }
    }
}

#[macro_export]
macro_rules! logln {
    () => {
       log!("/n");
    };
    ($($arg:tt)*) => {{
        $crate::log::inner_log(&format!($($arg)*));
    }};
}

pub use logln;

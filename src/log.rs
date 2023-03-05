use std::{ffi::{CStr, c_char, CString}, sync::RwLock};

static mut loading_logger: std::sync::RwLock<Option<FFGLLogger>> = RwLock::new(None);
// #[repr("C")]
type FFGLLogger = unsafe extern "C" fn(*const c_char) -> ();

#[no_mangle]
pub extern "C" fn SetLogCallback(logCallback: FFGLLogger) {
    unsafe { *loading_logger.write().unwrap() = Some(logCallback) };
}

pub fn inner_log(str: &str) {
    unsafe {
        if let Some(logger) = *loading_logger.read().unwrap() {
            let str = &CString::new(str).unwrap();
            logger(str.as_ptr());
        } else {
            eprintln!("{}", str);
        }
    }
}

macro_rules! logln {
    () => {
       log!("/n");
    };
    ($($arg:tt)*) => {{
        crate::log::inner_log(&format!($($arg)*));
    }};
}

pub(crate) use logln;
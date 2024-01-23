use std::{
    ffi::{c_char, CString},
    io,
    sync::RwLock,
};

pub static mut LOADING_LOGGER: std::sync::RwLock<Option<FFGLLogger>> = RwLock::new(None);
pub type FFGLLogger = unsafe extern "C" fn(*const c_char) -> ();

struct FFGLWriter;

impl io::Write for FFGLWriter {
    fn write(&mut self, buf: &[u8]) -> io::Result<usize> {
        let str = std::str::from_utf8(buf).unwrap();
        inner_log(str);
        Ok(buf.len())
    }

    fn flush(&mut self) -> io::Result<()> {
        Ok(())
    }
}

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

pub fn init_logger(logger: FFGLLogger) {
    unsafe { *LOADING_LOGGER.write().unwrap() = Some(logger) };

    std::panic::set_hook(Box::new(|cause| {
        tracing::error!("{}", cause);
    }));

    let env_filter = tracing_subscriber::EnvFilter::builder()
        .with_default_directive(LevelFilter::INFO.into())
        .from_env_lossy();

    //try set tracing logger
    if let Err(err) = tracing_subscriber::fmt()
        .with_writer(|| FFGLWriter)
        .without_time()
        .with_file(true)
        .with_line_number(true)
        .with_env_filter(env_filter)
        .finish()
        .try_init()
    {
        tracing::debug!("Failed to initialize logger: {}", err);
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

// pub use logln;
use tracing_subscriber::{filter::LevelFilter, util::SubscriberInitExt};

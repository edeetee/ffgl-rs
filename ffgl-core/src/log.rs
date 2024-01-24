use std::{
    ffi::{c_char, CStr, CString},
    io,
    sync::RwLock,
};

pub static mut LOADING_LOGGER: std::sync::RwLock<Option<FFGLLogger>> = RwLock::new(None);
pub type FFGLLogger = unsafe extern "C" fn(*const c_char) -> ();

struct FFGLWriter;

impl io::Write for FFGLWriter {
    fn write(&mut self, buf: &[u8]) -> io::Result<usize> {
        let str = String::from_utf8_lossy(buf);

        if let Some(logger) = unsafe { *LOADING_LOGGER.read().unwrap() } {
            let str = CString::new(str.as_bytes()).expect("Failed to convert to CString");
            unsafe { logger(str.as_ptr()) };
        } else {
            eprintln!("{}", str);
        }

        Ok(buf.len())
    }

    fn flush(&mut self) -> io::Result<()> {
        Ok(())
    }
}

pub fn init_default_subscriber() {
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

pub fn init_logger(logger: FFGLLogger) {
    unsafe { *LOADING_LOGGER.write().unwrap() = Some(logger) };

    std::panic::set_hook(Box::new(|cause| {
        tracing::error!("{}", cause);
    }));
}

// pub use logln;
use tracing_subscriber::{filter::LevelFilter, util::SubscriberInitExt};

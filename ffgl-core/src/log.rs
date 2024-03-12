//! Used to connect the internal FFGL logging system to your rust code.
//!
//!

use std::{
    ffi::{c_char, CString},
    io,
    sync::RwLock,
};

static mut LOADING_LOGGER: std::sync::RwLock<Option<FFGLLogger>> = RwLock::new(None);
///Type of the logging function the plugin can call
#[doc(hidden)]
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

pub(crate) fn try_init_default_subscriber() -> Result<(), tracing_subscriber::util::TryInitError> {
    let env_filter = tracing_subscriber::EnvFilter::builder()
        .with_default_directive(LevelFilter::INFO.into())
        .from_env_lossy();

    //try set tracing logger
    tracing_subscriber::fmt()
        .with_writer(|| FFGLWriter)
        .without_time()
        .with_file(true)
        .with_line_number(true)
        .with_env_filter(env_filter)
        .finish()
        .try_init()
}

///Initializes the default subscriber for the logger
///Will be automatically initialised after [crate::traits::FFGLHandler::init] is called
pub fn init_default_subscriber() {
    if let Err(err) = try_init_default_subscriber() {
        tracing::debug!("Failed to initialize logger: {}", err);
    }
}

///Only called by the plugin loader
#[doc(hidden)]
pub fn init_logger(logger: FFGLLogger) {
    unsafe { *LOADING_LOGGER.write().unwrap() = Some(logger) };

    std::panic::set_hook(Box::new(|cause| {
        tracing::error!("{}", cause);
    }));
}

use tracing_subscriber::{filter::LevelFilter, util::SubscriberInitExt};

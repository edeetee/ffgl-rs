use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};

use crate::ffi::ffgl2::*;

///Standard data that hosts provide to all programs
#[derive(Debug)]
pub struct FFGLData {
    pub created_at: Instant,
    pub viewport: FFGLViewportStruct,
    pub host_time: SystemTime,
    pub host_beat: SetBeatinfoStruct,
    // pub ctx:
}

impl FFGLData {
    pub fn new(viewport: &FFGLViewportStruct) -> FFGLData {
        Self {
            created_at: Instant::now(),
            viewport: viewport.clone(),
            host_time: SystemTime::now(),
            host_beat: SetBeatinfoStruct {
                bpm: 120.0,
                barPhase: 0.0,
            },
        }
    }

    pub fn set_beat(&mut self, beat: SetBeatinfoStruct) {
        self.host_beat = beat;
    }

    pub fn set_time(&mut self, host_seconds: f64) {
        self.host_time = UNIX_EPOCH + Duration::from_secs_f64(host_seconds / 1000.0)
    }

    pub fn get_dimensions(&self) -> (u32, u32) {
        (self.viewport.width, self.viewport.height)
    }
}

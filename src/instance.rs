use std::time::{Instant, Duration, UNIX_EPOCH, SystemTime};

use crate::ffgl2;

#[derive(Debug)]
pub struct FFGLInstance {
    pub created_at: Instant,
    pub viewport: crate::ffgl::FFGLViewportStruct,
    pub host_time: SystemTime,
    pub host_beat: ffgl2::SetBeatinfoStruct
}

impl FFGLInstance {
    pub fn new(viewport: &crate::ffgl::FFGLViewportStruct) -> FFGLInstance {
        Self {
            created_at: Instant::now(),
            viewport: viewport.clone(),
            host_time: SystemTime::now(),
            host_beat: ffgl2::SetBeatinfoStruct {
                bpm: 120.0,
                barPhase: 0.0,
            }
        }
    }

    pub fn set_beat(&mut self, beat: ffgl2::SetBeatinfoStruct) {
        self.host_beat = beat;
    }

    pub fn set_time(&mut self, host_seconds: f64) {
        self.host_time = UNIX_EPOCH + Duration::from_secs_f64(host_seconds/1000.0)
    }

    pub fn get_dimensions(&self) -> (u32, u32) {
        (self.viewport.width, self.viewport.height)
    }
}
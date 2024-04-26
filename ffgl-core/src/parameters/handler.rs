

use super::{info::ParamInfo, info::ParamValue};

///Handle the info for a collection of parameters. Allows for nested parameters.
pub trait ParamInfoHandler {
    fn num_params(&self) -> usize;
    fn param_info(&self, index: usize) -> &dyn ParamInfo;
}
///Handle collection of parameter values. Allows for nested parameters.
///
/// Maybe theres some way to just use a Index and IndexMut implementation for this instead?
pub trait ParamValueHandler {
    fn get_param(&self, index: usize) -> f32;
    fn set_param(&mut self, index: usize, value: f32);
}

impl<T> ParamValueHandler for [T]
where
    T: ParamValueHandler + ParamInfoHandler,
{
    fn get_param(&self, index: usize) -> f32 {
        let mut index = index;
        for p in self.iter() {
            if index < p.num_params() {
                return p.get_param(index);
            }
            index -= p.num_params();
        }
        panic!("Index out of bounds");
    }

    fn set_param(&mut self, index: usize, value: f32) {
        let mut index = index;
        for p in self.iter_mut() {
            if index < p.num_params() {
                p.set_param(index, value);
                return;
            }
            index -= p.num_params();
        }
        panic!("Index out of bounds");
    }
}

impl<T> ParamInfoHandler for [T]
where
    T: ParamInfoHandler,
{
    fn num_params(&self) -> usize {
        self.iter().map(|p| p.num_params()).sum()
    }

    fn param_info(&self, index: usize) -> &dyn ParamInfo {
        let mut index = index;
        for p in self.iter() {
            if index < p.num_params() {
                return p.param_info(index);
            }
            index -= p.num_params();
        }
        panic!("Index out of bounds");
    }
}

impl<T: ParamInfo> ParamInfoHandler for T {
    fn num_params(&self) -> usize {
        1
    }

    fn param_info(&self, _index: usize) -> &dyn ParamInfo {
        self
    }
}

impl<T: ParamValue> ParamValueHandler for T {
    fn get_param(&self, _index: usize) -> f32 {
        self.get()
    }

    fn set_param(&mut self, _index: usize, value: f32) {
        self.set(value)
    }
}

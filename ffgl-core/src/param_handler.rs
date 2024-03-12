use std::path::Iter;

use crate::parameters::{ParamInfo, ParamValue, SimpleParamInfo};

impl<T, P> ParamHandler<P> for Vec<T>
where
    T: ParamHandler<P>,
    P: ParamInfo,
{
    fn num_params(&self) -> usize {
        self.iter().map(|p| p.num_params()).sum()
    }

    fn param_info(&self, index: usize) -> &P {
        let mut index = index;
        for p in self.iter() {
            if index < p.num_params() {
                return p.param_info(index);
            }
            index -= p.num_params();
        }
        panic!("Index out of bounds");
    }

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

///Handle a collection of parameters. Allows for nested parameters.
pub trait ParamHandler<P: ParamInfo> {
    fn num_params(&self) -> usize;
    fn param_info(&self, index: usize) -> &P;
    fn get_param(&self, index: usize) -> f32;
    fn set_param(&mut self, index: usize, value: f32);
}

impl<T: ParamInfo> ParamHandler<T> for (T, f32) {
    fn num_params(&self) -> usize {
        1
    }

    fn param_info(&self, index: usize) -> &T {
        &self.0
    }

    fn get_param(&self, index: usize) -> f32 {
        self.1
    }

    fn set_param(&mut self, index: usize, value: f32) {
        self.1 = value;
    }
}

impl<T: ParamInfo + ParamValue> ParamHandler<T> for T {
    fn num_params(&self) -> usize {
        1
    }

    fn param_info(&self, index: usize) -> &Self {
        self
    }

    fn get_param(&self, index: usize) -> f32 {
        self.get()
    }

    fn set_param(&mut self, index: usize, value: f32) {
        self.set(value)
    }
}

use build_common::isf_glsl_preprocess::validate_isf_source;
use std::{env, fs, path::Path, process, thread, time::Duration};

fn main() {
    let args: Vec<String> = env::args().collect();

    if args.len() < 2 {
        eprintln!("Usage: {} <path_to_isf_shader>", args[0]);
        process::exit(1);
    }

    let file_path = &args[1];
    let path = Path::new(file_path);

    if !path.exists() {
        eprintln!("Error: File '{}' not found", file_path);
        process::exit(1);
    }

    // println!("Waiting for debugger to attach...");
    // println!("Press Ctrl+C to cancel.");

    // Wait for the debugger to attach
    // When debugging, you can set a breakpoint here and continue
    // when your debugger attaches
    // for i in (1..=30).rev() {
    //     print!("\rWaiting for {i} seconds... ");
    //     thread::sleep(Duration::from_secs(1));
    // }
    // println!("\rDebugger wait timeout elapsed. Continuing execution.");

    println!("Reading file: {}", file_path);
    let source = match fs::read_to_string(path) {
        Ok(content) => content,
        Err(e) => {
            eprintln!("Error reading file: {}", e);
            process::exit(1);
        }
    };

    println!("Validating ISF source...");
    match validate_isf_source(&source) {
        Ok(_) => println!("File is valid ISF!"),
        Err(e) => println!("Validation failed: {}", e),
    }
}

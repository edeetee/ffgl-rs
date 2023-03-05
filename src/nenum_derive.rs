

macro_rules! enum_const {
    ( 
        $ty:ty,
        $vis:vis enum $enum:ident { 
            $( $item:ident ), 
            *
            // $(item:ident = $expr:expr),*
            $(,)*
        } 
    ) => {					
        #[derive(Debug)]
        $vis enum $enum {
            $(
                $item,
            )*
        }

        impl TryFrom<$ty> for $enum {
            type Error = ();

            fn try_from(value: $ty) -> Result<$enum, Self::Error>{
                match value {
                    $(
                        $item => Ok($enum::$item),
                    )*
                    _ => Err(())
                }
            }
        }
        
        impl Into<$ty> for $enum {
            fn into(self) -> $ty {
                match self {
                    $(
                        $enum::$item => $item,
                    )*
                }
            }
        }
    }
}

pub(crate) use enum_const;
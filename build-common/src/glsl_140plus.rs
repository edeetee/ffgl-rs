use glsl::{
    syntax::{Expr, FunIdentifier, Identifier, StorageQualifier},
    visitor::VisitorMut,
};

pub struct Glsl140PlusMutator;

impl VisitorMut for Glsl140PlusMutator {
    fn visit_expr(&mut self, e: &mut glsl::syntax::Expr) -> glsl::visitor::Visit {
        match e {
            Expr::FunCall(FunIdentifier::Identifier(Identifier(f)), _) if f == "texture2D" => {
                *f = "texture".to_string();
            }
            _ => {}
        }

        glsl::visitor::Visit::Children
    }

    fn visit_storage_qualifier(
        &mut self,
        s: &mut glsl::syntax::StorageQualifier,
    ) -> glsl::visitor::Visit {
        match s {
            StorageQualifier::Attribute => *s = StorageQualifier::In,
            StorageQualifier::Varying => *s = StorageQualifier::Out,
            _ => {}
        }

        glsl::visitor::Visit::Children
    }
}

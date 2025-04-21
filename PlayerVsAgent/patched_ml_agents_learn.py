import types
import sys
import torch
import torch_directml

# Apply monkey patch to create torch.privateuseone before any other imports.
_dml = types.ModuleType("torch.privateuseone")
_dml.device = lambda index=0: torch_directml.device(index)
torch.privateuseone = _dml
sys.modules["torch.privateuseone"] = _dml

# Now import and run ML-Agents.
from mlagents.trainers import learn

if __name__ == "__main__":
    learn.main()
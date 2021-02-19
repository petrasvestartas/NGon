from compas.geometry import cross_vectors
from compas.geometry import normalize_vector
from compas.geometry import subtract_vectors

from compas_fea.cad import rhino
from compas_fea.structure import ElasticIsotropic
from compas_fea.structure import ElementProperties as Properties
from compas_fea.structure import FixedDisplacement
from compas_fea.structure import GeneralStep
from compas_fea.structure import PointLoad
from compas_fea.structure import CircularSection
from compas_fea.structure import Structure

import rhinoscriptsyntax as rs
import json


# Author: Aryan R. Rad


# Local Coordinate System per each element
for i in rs.ObjectsByLayer('elset_beams'):
    ez = subtract_vectors(rs.CurveEndPoint(i), rs.CurveStartPoint(i))
    ex = normalize_vector(cross_vectors(ez, [0, 0, 1]))
    rs.ObjectName(i, '_{0}'.format(json.dumps({'ex': ex})))
#-------------------------------------------------------------------------------


# Structure
# For the path in the command below, select the location you prefer
mdl = Structure(name='Nexorades', path='C:/TEMP/')
#-------------------------------------------------------------------------------


# Elements
rhino.add_nodes_elements_from_layers(mdl, line_type='BeamElement', layers='elset_beams')
#-------------------------------------------------------------------------------


# Sets
rhino.add_sets_from_layers(mdl, layers=['nset_support', 'nset_load'])
#-------------------------------------------------------------------------------


# Materials
mdl.add(ElasticIsotropic(name='mat_elastic', E=11000000, v=10**(-5), p=0.01))
#-------------------------------------------------------------------------------


# Sections
mdl.add(CircularSection(name='sec_beam', r=0.2))
#-------------------------------------------------------------------------------


# Properties
mdl.add(Properties(name='ep_beam', material='mat_elastic', section='sec_beam', elset='elset_beams'))
#-------------------------------------------------------------------------------


# Displacements
mdl.add(FixedDisplacement(name='disp_fixed', nodes='nset_support'))
#-------------------------------------------------------------------------------


# Loads
mdl.add(PointLoad(name='load_point', nodes='nset_load', z=-1100.99))
#-------------------------------------------------------------------------------


# Steps
mdl.add([
    GeneralStep(name='step_bc', displacements=['disp_fixed']),
    GeneralStep(name='step_load', loads=['load_point']),
])
mdl.steps_order = ['step_bc', 'step_load']
#-------------------------------------------------------------------------------


# Summary
mdl.summary()
#-------------------------------------------------------------------------------


# Run
mdl.analyse_and_extract(software='opensees', fields=['u', 'sf', 'sm','cf','rf'])
#mdl.analyse_and_extract(software='abaqus', fields=['u', 'sf', 'cf', 'rf', 's'], components=['ux', 'uy', 'uz', 'rfx', 'rfy', 'rfz', 'cfx', 'cfy', 'cfz', 'sxx', 'syy', 'smises'])
#-------------------------------------------------------------------------------


rhino.plot_data(mdl, step='step_load', field='uz', radius=0.01, scale=1.0)

rhino.plot_data(mdl, step='step_load', field='sf1', radius=0.01, scale=1.0)
rhino.plot_data(mdl, step='step_load', field='sf2', radius=0.01, scale=1.0)
rhino.plot_data(mdl, step='step_load', field='sf3', radius=0.01, scale=1.0)

rhino.plot_reaction_forces(mdl, step='step_load', scale=1.0)
rhino.plot_concentrated_forces(mdl, step='step_load', scale=1.0)
#-------------------------------------------------------------------------------

#rhino.plot_data(mdl, step='step_load', field='uz', radius=0.01, scale=1.0)

#rhino.plot_data(mdl, step='step_load', field='sxx', radius=0.01, scale=1.0)
#rhino.plot_data(mdl, step='step_load', field='syy', radius=0.01, scale=1.0)

#rhino.plot_reaction_forces(mdl, step='step_load', scale=1.0)
#rhino.plot_concentrated_forces(mdl, step='step_load', scale=1.0)

#rhino.plot_data(mdl, step='step_load', field='smises', radius=0.01)
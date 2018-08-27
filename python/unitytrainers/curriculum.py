import os
import json

from .exception import CurriculumError

import logging

logger = logging.getLogger('unitytrainers')


class Curriculum(object):
    def __init__(self, location, default_reset_parameters):
        """
        Initializes a Curriculum object.
        :param location: Path to JSON defining curriculum.
        :param default_reset_parameters: Set of reset parameters for environment.
        """
        self.lesson_length = 0
        self.max_lesson_num = 0
        self.measure = None
        self._lesson_num = 0
        # The name of the brain should be the basename of the file without the
        # extension.
        self._brain_name = os.path.basename(location).split('.')[0]

        try:
            with open(location) as data_file:
                self.data = json.load(data_file)
        except IOError:
            raise CurriculumError(
                'The file {0} could not be found.'.format(location))
        except UnicodeDecodeError:
            raise CurriculumError('There was an error decoding {}'.format(location))
        self.smoothing_value = 0
        for key in ['parameters', 'measure', 'thresholds',
                    'min_lesson_length', 'signal_smoothing']:
            if key not in self.data:
                raise CurriculumError("{0} does not contain a "
                                                "{1} field.".format(location, key))
        self.smoothing_value = 0
        self.measure = self.data['measure']
        self.max_lesson_num = len(self.data['thresholds'])

        parameters = self.data['parameters']
        for key in parameters:
            if key not in default_reset_parameters:
                raise CurriculumError(
                    'The parameter {0} in Curriculum {1} is not present in '
                    'the Environment'.format(key, location))
            if len(parameters[key]) != self.max_lesson_num + 1:
                raise CurriculumError(
                    'The parameter {0} in Curriculum {1} must have {2} values '
                    'but {3} were found'.format(key, location,
                                                self.max_lesson_num + 1, len(parameters[key])))

    @property
    def lesson_num(self):
        return self._lesson_num

    @lesson_num.setter
    def lesson_num(self, lesson_num):
        self.lesson_length = 0
        self._lesson_num = max(0, min(lesson_num, self.max_lesson_num))

    def increment_lesson(self, progress):
        """
        Increments the lesson number depending on the progress given.
        :param progress: Measure of progress (either reward or percentage steps completed).
        """
        if self.data is None or progress is None:
            return
        if self.data['signal_smoothing']:
            progress = self.smoothing_value * 0.25 + 0.75 * progress
            self.smoothing_value = progress
        self.lesson_length += 1
        if self.lesson_num < self.max_lesson_num:
            if ((progress > self.data['thresholds'][self.lesson_num]) and
                    (self.lesson_length > self.data['min_lesson_length'])):
                self.lesson_length = 0
                self.lesson_num += 1
                config = {}
                parameters = self.data['parameters']
                for key in parameters:
                    config[key] = parameters[key][self.lesson_num]
                logger.info('{0} lesson changed. Now in lesson {1}: {2}'
                            .format(self._brain_name,
                                    self.lesson_num,
                                    ', '.join([str(x) + ' -> ' + str(config[x]) for x in config])))

    def get_config(self, lesson=None):
        """
        Returns reset parameters which correspond to the lesson.
        :param lesson: The lesson you want to get the config of. If None, the current lesson is returned.
        :return: The configuration of the reset parameters.
        """
        if self.data is None:
            return {}
        if lesson is None:
            lesson = self.lesson_num
        lesson = max(0, min(lesson, self.max_lesson_num))
        config = {}
        parameters = self.data['parameters']
        for key in parameters:
            config[key] = parameters[key][lesson]
        return config
